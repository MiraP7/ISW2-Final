using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ISW2_Primer_parcial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly string _uploadPath;

        public FileUploadController()
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        /// <summary>
        /// Subir un nuevo archivo
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No se ha proporcionado ningún archivo." });

            // Validar tipo de archivo
            if (!IsValidFileType(file.FileName, out string extension))
            {
                return BadRequest(new 
                { 
                    message = "Tipo de archivo no permitido.",
                    allowedTypes = GetAllowedTypesMessage(),
                    receivedExtension = extension
                });
            }

            var filePath = Path.Combine(_uploadPath, file.FileName);

            // Verificar si el archivo ya existe
            if (System.IO.File.Exists(filePath))
            {
                return Conflict(new { message = $"El archivo '{file.FileName}' ya existe. Use PUT para actualizarlo." });
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return CreatedAtAction(nameof(GetFile), new { fileName = file.FileName }, new 
            { 
                fileName = file.FileName, 
                size = file.Length,
                path = filePath,
                message = "Archivo subido exitosamente"
            });
        }

        /// <summary>
        /// Obtener lista de todos los archivos subidos
        /// </summary>
        [HttpGet("files")]
        public IActionResult GetAllFiles()
        {
            if (!Directory.Exists(_uploadPath))
            {
                return Ok(new { files = new string[] { }, count = 0, message = "No hay archivos subidos" });
            }

            var files = Directory.GetFiles(_uploadPath)
                .Select(f => new 
                {
                    fileName = Path.GetFileName(f),
                    size = new FileInfo(f).Length,
                    sizeFormatted = FormatFileSize(new FileInfo(f).Length),
                    uploadDate = System.IO.File.GetCreationTime(f),
                    lastModified = System.IO.File.GetLastWriteTime(f)
                })
                .OrderByDescending(f => f.uploadDate)
                .ToList();

            return Ok(new { files, count = files.Count });
        }

        /// <summary>
        /// Obtener información de un archivo específico
        /// </summary>
        [HttpGet("files/{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"El archivo '{fileName}' no existe." });
            }

            var fileInfo = new FileInfo(filePath);
            
            return Ok(new 
            {
                fileName = fileInfo.Name,
                size = fileInfo.Length,
                sizeFormatted = FormatFileSize(fileInfo.Length),
                uploadDate = fileInfo.CreationTime,
                lastModified = fileInfo.LastWriteTime,
                extension = fileInfo.Extension,
                path = filePath
            });
        }

        /// <summary>
        /// Descargar un archivo específico
        /// </summary>
        [HttpGet("download/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"El archivo '{fileName}' no existe." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/octet-stream";

            return File(fileBytes, contentType, fileName);
        }

        /// <summary>
        /// Actualizar/reemplazar un archivo existente
        /// </summary>
        [HttpPut("update/{fileName}")]
        public async Task<IActionResult> UpdateFile(string fileName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No se ha proporcionado ningún archivo." });

            // Validar tipo de archivo
            if (!IsValidFileType(file.FileName, out string extension))
            {
                return BadRequest(new 
                { 
                    message = "Tipo de archivo no permitido.",
                    allowedTypes = GetAllowedTypesMessage(),
                    receivedExtension = extension
                });
            }

            var filePath = Path.Combine(_uploadPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"El archivo '{fileName}' no existe. Use POST para crearlo." });
            }

            // Eliminar el archivo existente
            System.IO.File.Delete(filePath);

            // Guardar el nuevo archivo con el mismo nombre
            var newFilePath = Path.Combine(_uploadPath, fileName);
            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new 
            { 
                fileName = fileName,
                newSize = file.Length,
                message = "Archivo actualizado exitosamente"
            });
        }

        /// <summary>
        /// Renombrar un archivo
        /// </summary>
        [HttpPatch("rename/{oldFileName}")]
        public IActionResult RenameFile(string oldFileName, [FromBody] RenameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.NewFileName))
            {
                return BadRequest(new { message = "Debe proporcionar un nuevo nombre de archivo." });
            }

            var oldPath = Path.Combine(_uploadPath, oldFileName);
            var newPath = Path.Combine(_uploadPath, request.NewFileName);

            if (!System.IO.File.Exists(oldPath))
            {
                return NotFound(new { message = $"El archivo '{oldFileName}' no existe." });
            }

            if (System.IO.File.Exists(newPath))
            {
                return Conflict(new { message = $"Ya existe un archivo con el nombre '{request.NewFileName}'." });
            }

            System.IO.File.Move(oldPath, newPath);

            return Ok(new 
            { 
                oldFileName = oldFileName,
                newFileName = request.NewFileName,
                message = "Archivo renombrado exitosamente"
            });
        }

        /// <summary>
        /// Eliminar un archivo específico
        /// </summary>
        [HttpDelete("delete/{fileName}")]
        public IActionResult DeleteFile(string fileName)
        {
            var filePath = Path.Combine(_uploadPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"El archivo '{fileName}' no existe." });
            }

            System.IO.File.Delete(filePath);

            return Ok(new 
            { 
                fileName = fileName,
                message = "Archivo eliminado exitosamente"
            });
        }

        /// <summary>
        /// Eliminar todos los archivos
        /// </summary>
        [HttpDelete("deleteAll")]
        public IActionResult DeleteAllFiles()
        {
            if (!Directory.Exists(_uploadPath))
            {
                return Ok(new { message = "No hay archivos para eliminar." });
            }

            var files = Directory.GetFiles(_uploadPath);
            
            if (files.Length == 0)
            {
                return Ok(new { message = "No hay archivos para eliminar." });
            }

            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }

            return Ok(new 
            { 
                deletedCount = files.Length,
                message = $"Se eliminaron {files.Length} archivo(s) exitosamente."
            });
        }

        // Método auxiliar para formatear el tamaño del archivo
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        // Método auxiliar para validar el tipo de archivo
        private bool IsValidFileType(string fileName, out string extension)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".odt", ".ods", ".odp" };
            extension = Path.GetExtension(fileName).ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension);
        }

        // Método auxiliar para obtener el mensaje de tipos permitidos
        private string GetAllowedTypesMessage()
        {
            return "PDF, Word (.doc, .docx), Excel (.xls, .xlsx), PowerPoint (.ppt, .pptx), Texto (.txt), OpenOffice (.odt, .ods, .odp)";
        }
    }

    // Modelo para renombrar archivos
    public class RenameRequest
    {
        public string NewFileName { get; set; } = string.Empty;
    }
}
