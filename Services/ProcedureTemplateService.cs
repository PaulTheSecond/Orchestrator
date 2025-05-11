using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrchestratorApp.Data;
using OrchestratorApp.Domain.Entities;
using OrchestratorApp.Domain.Enums;
using OrchestratorApp.DTOs;
using System.Text.Json.Serialization;

namespace OrchestratorApp.Services
{
    public class ProcedureTemplateService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ProcedureTemplateService> _logger;

        public ProcedureTemplateService(ApplicationDbContext dbContext, ILogger<ProcedureTemplateService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<ProcedureTemplateDTO>> GetAllProcedureTemplatesAsync()
        {
            var procedureTemplates = await _dbContext.ProcedureTemplates
                .Include(p => p.ProcedureStages)
                .AsNoTracking()
                .ToListAsync();

            return procedureTemplates.Select(MapToDTO);
        }

        public async Task<ProcedureTemplateDTO?> GetProcedureTemplateAsync(Guid id)
        {
            try
            {
                // First get the procedure template with its stages
                var procedureTemplate = await _dbContext.ProcedureTemplates
                    .Include(p => p.ProcedureStages)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (procedureTemplate == null)
                {
                    return null;
                }

                // Then separately get the contest templates to avoid the StatusModelArray issue
                var contestTemplates = await _dbContext.ContestTemplates
                    .Include(c => c.Stages)
                    .Where(c => c.ProcedureTemplateId == id)
                    .AsNoTracking()
                    .ToListAsync();

                procedureTemplate.ContestTemplates = contestTemplates;

                return MapToDTO(procedureTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving procedure template with ID {Id}", id);
                throw;
            }
        }

        public async Task<ProcedureTemplateDTO> CreateProcedureTemplateAsync(ProcedureTemplateDTO dto)
        {
            // Validate the procedure template
            ValidateProcedureTemplate(dto);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Create the procedure template
                var procedureTemplate = new ProcedureTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Version = dto.Version,
                    IsPublished = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.ProcedureTemplates.AddAsync(procedureTemplate);
                await _dbContext.SaveChangesAsync();

                // Create procedure stages with proper linking
                foreach (var stageDto in dto.ProcedureStages.OrderBy(s => s.Order))
                {
                    var stage = new ProcedureStageTemplate
                    {
                        Id = Guid.NewGuid(),
                        ProcedureTemplateId = procedureTemplate.Id,
                        StageType = stageDto.StageType,
                        Order = stageDto.Order,
                        DefaultServiceName = stageDto.DefaultServiceName
                    };

                    await _dbContext.ProcedureStageTemplates.AddAsync(stage);
                }

                await _dbContext.SaveChangesAsync();

                // Update previous/next stage IDs
                var stages = await _dbContext.ProcedureStageTemplates
                    .Where(s => s.ProcedureTemplateId == procedureTemplate.Id)
                    .OrderBy(s => s.Order)
                    .ToListAsync();

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
                await transaction.CommitAsync();

                // Reload the procedure template with stages
                return await GetProcedureTemplateAsync(procedureTemplate.Id) ?? 
                    throw new Exception("Failed to retrieve the created procedure template");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating procedure template");
                throw;
            }
        }

        public async Task<ProcedureTemplateDTO> UpdateProcedureTemplateAsync(Guid id, ProcedureTemplateDTO dto)
        {
            var procedureTemplate = await _dbContext.ProcedureTemplates
                .Include(p => p.ProcedureStages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (procedureTemplate == null)
            {
                throw new KeyNotFoundException($"Procedure template with ID {id} not found");
            }

            if (procedureTemplate.IsPublished)
            {
                throw new InvalidOperationException("Cannot update a published procedure template");
            }

            // Validate the procedure template
            ValidateProcedureTemplate(dto);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Update basic properties
                procedureTemplate.Name = dto.Name;
                procedureTemplate.UpdatedAt = DateTime.UtcNow;

                // Delete existing stages
                _dbContext.ProcedureStageTemplates.RemoveRange(procedureTemplate.ProcedureStages);
                await _dbContext.SaveChangesAsync();

                // Create new stages
                foreach (var stageDto in dto.ProcedureStages.OrderBy(s => s.Order))
                {
                    var stage = new ProcedureStageTemplate
                    {
                        Id = Guid.NewGuid(),
                        ProcedureTemplateId = procedureTemplate.Id,
                        StageType = stageDto.StageType,
                        Order = stageDto.Order,
                        DefaultServiceName = stageDto.DefaultServiceName
                    };

                    await _dbContext.ProcedureStageTemplates.AddAsync(stage);
                }

                await _dbContext.SaveChangesAsync();

                // Update previous/next stage IDs
                var stages = await _dbContext.ProcedureStageTemplates
                    .Where(s => s.ProcedureTemplateId == procedureTemplate.Id)
                    .OrderBy(s => s.Order)
                    .ToListAsync();

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
                await transaction.CommitAsync();

                // Reload the procedure template with stages
                return await GetProcedureTemplateAsync(procedureTemplate.Id) ?? 
                    throw new Exception("Failed to retrieve the updated procedure template");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating procedure template");
                throw;
            }
        }

        public async Task<ProcedureTemplateDTO> PublishProcedureTemplateAsync(Guid id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var procedureTemplate = await _dbContext.ProcedureTemplates
                    .Include(p => p.ProcedureStages)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (procedureTemplate == null)
                {
                    throw new KeyNotFoundException($"Procedure template with ID {id} not found");
                }

                if (procedureTemplate.IsPublished)
                {
                    throw new InvalidOperationException("Procedure template is already published");
                }

                // Validate that the template has a Contests stage
                var hasContestsStage = procedureTemplate.ProcedureStages
                    .Any(s => s.StageType == ProcedureStageType.Contests);

                if (!hasContestsStage)
                {
                    throw new InvalidOperationException("Procedure template must include a Contests stage");
                }

                // Validate stage sequence
                var stages = procedureTemplate.ProcedureStages.OrderBy(s => s.Order).ToList();
                for (int i = 0; i < stages.Count; i++)
                {
                    if (i > 0 && stages[i].PreviousStageId != stages[i - 1].Id)
                    {
                        throw new InvalidOperationException("Invalid stage sequence: previous stage references don't match");
                    }

                    if (i < stages.Count - 1 && stages[i].NextStageId != stages[i + 1].Id)
                    {
                        throw new InvalidOperationException("Invalid stage sequence: next stage references don't match");
                    }
                }

                // Unpublish any existing published version with the same name
                var existingPublished = await _dbContext.ProcedureTemplates
                    .Where(p => p.Name == procedureTemplate.Name && p.IsPublished && p.Id != id)
                    .ToListAsync();

                foreach (var published in existingPublished)
                {
                    published.IsPublished = false;
                    published.UpdatedAt = DateTime.UtcNow;
                }

                // Publish the current template
                procedureTemplate.IsPublished = true;
                procedureTemplate.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetProcedureTemplateAsync(id) ?? 
                    throw new Exception("Failed to retrieve the published procedure template");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error publishing procedure template");
                throw;
            }
        }

        public async Task DeleteProcedureTemplateAsync(Guid id)
        {
            var procedureTemplate = await _dbContext.ProcedureTemplates
                .Include(p => p.ProcedureStages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (procedureTemplate == null)
            {
                throw new KeyNotFoundException($"Procedure template with ID {id} not found");
            }

            if (procedureTemplate.IsPublished)
            {
                throw new InvalidOperationException("Cannot delete a published procedure template");
            }

            // Check for any existing procedure instances
            bool hasInstances = await _dbContext.ProcedureInstances
                .AnyAsync(p => p.ProcedureTemplateId == id);

            if (hasInstances)
            {
                throw new InvalidOperationException("Cannot delete a procedure template that has instances");
            }

            _dbContext.ProcedureStageTemplates.RemoveRange(procedureTemplate.ProcedureStages);
            _dbContext.ProcedureTemplates.Remove(procedureTemplate);
            await _dbContext.SaveChangesAsync();
        }

        private void ValidateProcedureTemplate(ProcedureTemplateDTO dto)
        {
            if (dto.ProcedureStages == null || !dto.ProcedureStages.Any())
            {
                throw new ArgumentException("Procedure template must have at least one stage");
            }

            // Check for Contests stage
            var hasContestsStage = dto.ProcedureStages.Any(s => s.StageType == ProcedureStageType.Contests);
            if (!hasContestsStage)
            {
                throw new ArgumentException("Procedure template must include a Contests stage");
            }

            // Check for duplicate stages
            var stageTypes = dto.ProcedureStages.Select(s => s.StageType).ToList();
            if (stageTypes.Count != stageTypes.Distinct().Count())
            {
                throw new ArgumentException("Duplicate stage types are not allowed");
            }

            // Check that order values are consecutive starting from 1
            var orders = dto.ProcedureStages.Select(s => s.Order).OrderBy(o => o).ToList();
            if (orders[0] != 1 || orders.Zip(orders.Skip(1), (a, b) => b - a).Any(diff => diff != 1))
            {
                throw new ArgumentException("Stage order must be consecutive starting from 1");
            }
        }

        private ProcedureTemplateDTO MapToDTO(ProcedureTemplate entity)
        {
            var dto = new ProcedureTemplateDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Version = entity.Version,
                IsPublished = entity.IsPublished,
                ProcedureStages = entity.ProcedureStages.Select(s => new ProcedureStageTemplateDTO
                {
                    Id = s.Id,
                    StageType = s.StageType,
                    Order = s.Order,
                    PreviousStageId = s.PreviousStageId,
                    NextStageId = s.NextStageId,
                    DefaultServiceName = s.DefaultServiceName
                }).ToList()
            };

            if (entity.ContestTemplates != null)
            {
                dto.ContestTemplates = entity.ContestTemplates.Select(c => new ContestTemplateDTO
                {
                    Id = c.Id,
                    ProcedureTemplateId = c.ProcedureTemplateId,
                    Name = c.Name,
                    Version = c.Version,
                    IsPublished = c.IsPublished,
                    StatusModelArray = c.StatusModelArray,
                    Stages = c.Stages.Select(s => new StageTemplateDTO
                    {
                        Id = s.Id,
                        StageType = s.StageType,
                        Order = s.Order,
                        PreviousStageId = s.PreviousStageId,
                        NextStageId = s.NextStageId,
                        DefaultServiceName = s.DefaultServiceName
                    }).ToList()
                }).ToList();
            }

            return dto;
        }
        
        /// <summary>
        /// Экспортирует шаблон процедуры со всеми связанными данными в формат JSON
        /// </summary>
        public async Task<TemplateExportDTO> ExportProcedureTemplateAsync(Guid id)
        {
            _logger.LogInformation("Exporting procedure template with ID {Id}", id);
            
            var procedureTemplate = await GetProcedureTemplateAsync(id);
            
            if (procedureTemplate == null)
            {
                throw new KeyNotFoundException($"Procedure template with ID {id} not found");
            }
            
            // Создаем объект экспорта
            var exportDto = new TemplateExportDTO
            {
                ExportDate = DateTime.UtcNow,
                ProcedureTemplate = procedureTemplate
            };
            
            return exportDto;
        }
        
        /// <summary>
        /// Импортирует шаблон процедуры со всеми связанными данными из формата JSON
        /// </summary>
        public async Task<ProcedureTemplateDTO> ImportProcedureTemplateAsync(TemplateExportDTO importDto)
        {
            _logger.LogInformation("Importing procedure template: {Name}", importDto.Name);
            
            // Проверяем, что содержимое импорта корректно
            if (importDto.ProcedureTemplate == null)
            {
                throw new ArgumentException("Import data does not contain a valid procedure template");
            }
            
            // Подготавливаем данные для импорта, генерируя новые идентификаторы
            var procedureDto = importDto.ProcedureTemplate;
            
            // Генерируем новые идентификаторы для основного шаблона и этапов
            procedureDto.Id = Guid.NewGuid();
            
            // Обновляем имя, чтобы избежать конфликтов
            procedureDto.Name = $"{procedureDto.Name} (Imported {DateTime.UtcNow:yyyy-MM-dd})";
            procedureDto.IsPublished = false; // Импортированный шаблон всегда не опубликован
            
            // Для каждого этапа процедуры генерируем новый идентификатор,
            // сохраняя оригинальный в словаре для последующего обновления связей
            var stageIdMap = new Dictionary<Guid, Guid>();
            
            foreach (var stage in procedureDto.ProcedureStages)
            {
                var originalId = stage.Id;
                var newId = Guid.NewGuid();
                stageIdMap[originalId] = newId;
                stage.Id = newId;
                
                // Очищаем связи, они будут восстановлены позже
                stage.PreviousStageId = null;
                stage.NextStageId = null;
            }
            
            // Восстанавливаем связи между этапами
            foreach (var stage in procedureDto.ProcedureStages)
            {
                if (stage.PreviousStageId.HasValue && stageIdMap.ContainsKey(stage.PreviousStageId.Value))
                {
                    stage.PreviousStageId = stageIdMap[stage.PreviousStageId.Value];
                }
                
                if (stage.NextStageId.HasValue && stageIdMap.ContainsKey(stage.NextStageId.Value))
                {
                    stage.NextStageId = stageIdMap[stage.NextStageId.Value];
                }
            }
            
            // Обрабатываем шаблоны конкурсов, если они есть
            if (procedureDto.ContestTemplates != null && procedureDto.ContestTemplates.Any())
            {
                var contestIdMap = new Dictionary<Guid, Guid>();
                
                foreach (var contest in procedureDto.ContestTemplates)
                {
                    var originalId = contest.Id;
                    var newId = Guid.NewGuid();
                    contestIdMap[originalId] = newId;
                    contest.Id = newId;
                    contest.ProcedureTemplateId = procedureDto.Id;
                    contest.IsPublished = false;
                    
                    // Обновляем идентификаторы этапов конкурса
                    var contestStageIdMap = new Dictionary<Guid, Guid>();
                    
                    foreach (var stage in contest.Stages)
                    {
                        var originalStageId = stage.Id;
                        var newStageId = Guid.NewGuid();
                        contestStageIdMap[originalStageId] = newStageId;
                        stage.Id = newStageId;
                        
                        // Очищаем связи
                        stage.PreviousStageId = null;
                        stage.NextStageId = null;
                    }
                    
                    // Восстанавливаем связи между этапами конкурса
                    foreach (var stage in contest.Stages)
                    {
                        if (stage.PreviousStageId.HasValue && contestStageIdMap.ContainsKey(stage.PreviousStageId.Value))
                        {
                            stage.PreviousStageId = contestStageIdMap[stage.PreviousStageId.Value];
                        }
                        
                        if (stage.NextStageId.HasValue && contestStageIdMap.ContainsKey(stage.NextStageId.Value))
                        {
                            stage.NextStageId = contestStageIdMap[stage.NextStageId.Value];
                        }
                    }
                }
            }
            
            // Создаем новый шаблон на основе подготовленных данных
            var result = await CreateProcedureTemplateAsync(procedureDto);
            
            _logger.LogInformation("Successfully imported procedure template with ID {Id}", result.Id);
            
            return result;
        }
    }
}
