using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrchestratorApp.Data;
using OrchestratorApp.Domain.Entities;
using OrchestratorApp.Domain.Enums;
using OrchestratorApp.Domain.Messaging;
using OrchestratorApp.DTOs;
using System.Text.Json;

namespace OrchestratorApp.Services
{
    public class ContestTemplateService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<ContestTemplateService> _logger;

        public ContestTemplateService(
            ApplicationDbContext dbContext,
            RabbitMQService rabbitMQService,
            ILogger<ContestTemplateService> logger)
        {
            _dbContext = dbContext;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task<IEnumerable<ContestTemplateDTO>> GetAllContestTemplatesAsync()
        {
            try {
                // Используем возможности Entity Framework Core
                var contestTemplates = await _dbContext.ContestTemplates
                    .Include(t => t.Stages)
                    .ToListAsync();
                
                return contestTemplates.Select(MapToDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contest templates");
                return Enumerable.Empty<ContestTemplateDTO>();
            }
        }

        public async Task<ContestTemplateDTO?> GetContestTemplateAsync(Guid id)
        {
            try
            {
                // Используем возможности Entity Framework Core
                var contestTemplate = await _dbContext.ContestTemplates
                    .Include(t => t.Stages)
                    .FirstOrDefaultAsync(t => t.Id == id);
                
                if (contestTemplate == null)
                    return null;
                
                return MapToDTO(contestTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving contest template with ID {id}");
                return null;
            }
        }

        public async Task<IEnumerable<ContestTemplateDTO>> GetContestTemplatesByProcedureAsync(Guid procedureTemplateId)
        {
            try
            {
                // Используем возможности Entity Framework Core
                var templates = await _dbContext.ContestTemplates
                    .Where(t => t.ProcedureTemplateId == procedureTemplateId)
                    .Include(t => t.Stages)
                    .OrderBy(t => t.Name)
                    .ToListAsync();
                
                return templates.Select(MapToDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving contest templates for procedure with ID {procedureTemplateId}");
                return Enumerable.Empty<ContestTemplateDTO>();
            }
        }

        public async Task<ContestTemplateDTO> CreateContestTemplateAsync(CreateContestTemplateDTO dto)
        {
            // Check if procedure template exists
            var procedureTemplate = await _dbContext.ProcedureTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProcedureTemplateId);
                
            if (procedureTemplate == null)
            {
                throw new KeyNotFoundException($"Procedure template with ID {dto.ProcedureTemplateId} not found");
            }
            
            // Validate stage order
            ValidateStageOrder(dto.Stages);
            
            // Create ContestTemplate entity
            var template = new ContestTemplate
            {
                Id = Guid.NewGuid(),
                ProcedureTemplateId = dto.ProcedureTemplateId,
                Name = dto.Name,
                Version = 1,
                IsPublished = false,
                StatusModelArray = dto.StatusModelArray
            };
            
            // Add template to database
            await _dbContext.ContestTemplates.AddAsync(template);
            
            // Create and add stages
            foreach (var stageDto in dto.Stages)
            {
                var stage = new StageTemplate
                {
                    Id = Guid.NewGuid(),
                    ContestTemplateId = template.Id,
                    StageType = (ContestStageType)(int)stageDto.StageType,
                    Order = stageDto.Order,
                    DefaultServiceName = stageDto.DefaultServiceName
                };
                
                await _dbContext.StageTemplates.AddAsync(stage);
            }
            
            // Save changes
            await _dbContext.SaveChangesAsync();
            
            // Link stages based on order
            await LinkStagesByOrderAsync(template.Id);
            
            // Get the complete template with linked stages
            var createdTemplate = await GetContestTemplateAsync(template.Id);
            
            if (createdTemplate == null)
            {
                throw new InvalidOperationException("Failed to retrieve created contest template");
            }
            
            return createdTemplate;
        }

        public async Task<ContestTemplateDTO> UpdateContestTemplateAsync(Guid id, UpdateContestTemplateDTO dto)
        {
            // Check if template exists
            var existingTemplate = await _dbContext.ContestTemplates
                .Include(t => t.Stages)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"Contest template with ID {id} not found");
            }
            
            if (existingTemplate.IsPublished)
            {
                throw new InvalidOperationException("Published templates cannot be modified");
            }
            
            // Validate stage order
            ValidateStageOrder(dto.Stages);
            
            // Update basic properties
            existingTemplate.Name = dto.Name;
            existingTemplate.StatusModelArray = dto.StatusModelArray;
            
            // Remove existing stages
            _dbContext.StageTemplates.RemoveRange(existingTemplate.Stages);
            
            // Create and add new stages
            foreach (var stageDto in dto.Stages)
            {
                var stage = new StageTemplate
                {
                    Id = Guid.NewGuid(),
                    ContestTemplateId = existingTemplate.Id,
                    StageType = (ContestStageType)(int)stageDto.StageType,
                    Order = stageDto.Order,
                    DefaultServiceName = stageDto.DefaultServiceName
                };
                
                await _dbContext.StageTemplates.AddAsync(stage);
            }
            
            // Save changes
            await _dbContext.SaveChangesAsync();
            
            // Link stages based on order
            await LinkStagesByOrderAsync(existingTemplate.Id);
            
            // Get the updated template
            var updatedTemplate = await GetContestTemplateAsync(existingTemplate.Id);
            
            if (updatedTemplate == null)
            {
                throw new InvalidOperationException("Failed to retrieve updated contest template");
            }
            
            return updatedTemplate;
        }

        public async Task<ContestTemplateDTO> PublishContestTemplateAsync(Guid id)
        {
            var template = await _dbContext.ContestTemplates
                .Include(t => t.Stages)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (template == null)
            {
                throw new KeyNotFoundException($"Contest template with ID {id} not found");
            }
            
            if (template.IsPublished)
            {
                return MapToDTO(template);
            }
            
            // Validate template before publishing
            if (template.Stages.Count == 0)
            {
                throw new InvalidOperationException("Cannot publish a template without any stages");
            }
            
            if (template.StatusModelArray == null || template.StatusModelArray.Length == 0)
            {
                throw new InvalidOperationException("Cannot publish a template without status model configuration");
            }
            
            // Update template
            template.IsPublished = true;
            _dbContext.Update(template);
            await _dbContext.SaveChangesAsync();
            
            // Publish template created event
            await _rabbitMQService.PublishEventAsync(new ContestTemplateCreatedEvent
            {
                TemplateId = template.Id,
                ProcedureTemplateId = template.ProcedureTemplateId,
                Name = template.Name,
                Version = template.Version,
                StageTypes = template.Stages.Select(s => s.StageType.ToString()).ToList()
            });
            
            return MapToDTO(template);
        }
        
        public async Task DeleteContestTemplateAsync(Guid id)
        {
            var template = await _dbContext.ContestTemplates
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (template == null)
            {
                throw new KeyNotFoundException($"Contest template with ID {id} not found");
            }
            
            if (template.IsPublished)
            {
                throw new InvalidOperationException("Cannot delete published templates");
            }
            
            // Check if the template has instances
            var hasInstances = await _dbContext.ContestInstances
                .AnyAsync(i => i.ContestTemplateId == id);
                
            if (hasInstances)
            {
                throw new InvalidOperationException("Cannot delete a template that has instances");
            }
            
            // Remove the template
            _dbContext.ContestTemplates.Remove(template);
            await _dbContext.SaveChangesAsync();
        }

        private void ValidateStageOrder<T>(IEnumerable<T> stages) where T : class
        {
            if (!stages.Any())
            {
                throw new ArgumentException("Contest template must have at least one stage");
            }
            
            var orders = new List<int>();
            var stageTypes = new List<ContestStageType>();
            
            // Handle different stage template DTO types
            foreach (var stage in stages)
            {
                if (stage is CreateStageTemplateDTO createDto)
                {
                    orders.Add(createDto.Order);
                    stageTypes.Add(createDto.StageType);
                }
                else if (stage is UpdateStageTemplateDTO updateDto)
                {
                    orders.Add(updateDto.Order);
                    stageTypes.Add(updateDto.StageType);
                }
            }
            
            if (orders.Count != orders.Distinct().Count())
            {
                throw new ArgumentException("Duplicate stage orders are not allowed");
            }
            
            if (stageTypes.Count != stageTypes.Distinct().Count())
            {
                throw new ArgumentException("Duplicate stage types are not allowed");
            }
            
            // Check if orders form a continuous sequence starting from 1
            var min = orders.Min();
            var max = orders.Max();
            
            if (min != 1)
            {
                throw new ArgumentException("Stage order must start from 1");
            }
            
            if (max - min + 1 != orders.Count)
            {
                throw new ArgumentException("Stage orders must form a continuous sequence");
            }
        }

        private async Task LinkStagesByOrderAsync(Guid templateId)
        {
            var stages = await _dbContext.StageTemplates
                .Where(s => s.ContestTemplateId == templateId)
                .OrderBy(s => s.Order)
                .ToListAsync();
                
            if (stages.Count <= 1)
                return;
                
            for (int i = 0; i < stages.Count; i++)
            {
                if (i > 0)
                {
                    stages[i].PreviousStageId = stages[i - 1].Id;
                }
                
                if (i < stages.Count - 1)
                {
                    stages[i].NextStageId = stages[i + 1].Id;
                }
            }
            
            await _dbContext.SaveChangesAsync();
        }

        private ContestTemplateDTO MapToDTO(ContestTemplate entity)
        {
            return new ContestTemplateDTO
            {
                Id = entity.Id,
                ProcedureTemplateId = entity.ProcedureTemplateId,
                Name = entity.Name,
                Version = entity.Version,
                IsPublished = entity.IsPublished,
                StatusModelArray = entity.StatusModelArray,
                Stages = entity.Stages?.Select(s => new StageTemplateDTO
                {
                    Id = s.Id,
                    StageType = s.StageType,
                    Order = s.Order,
                    PreviousStageId = s.PreviousStageId,
                    NextStageId = s.NextStageId,
                    DefaultServiceName = s.DefaultServiceName
                }).ToList()
            };
        }
    }
}