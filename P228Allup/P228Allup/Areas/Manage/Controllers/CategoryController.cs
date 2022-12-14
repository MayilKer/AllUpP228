using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P228Allup.DAL;
using P228Allup.Extension;
using P228Allup.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace P228Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> categories = await _context.Categories
                .Include(c=>c.Products)
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            if (category.IsMain)
            {
                if (category.File == null)
                {
                    ModelState.AddModelError("File", "Fayl Mecburidi");
                    return View(category);
                }

                if (!category.File.CheckFileSize(1000))
                {
                    ModelState.AddModelError("File", "Fayl Olcusu maksimum 1000 kb olmalidir");
                    return View(category);
                }

                if (!category.File.CheckFileType("image/jpeg"))
                {
                    ModelState.AddModelError("File", "Fayl Tipi .jpg ve ya .jpeg olmalidir");
                    return View(category);
                }

                category.ParentId = null;
                category.Image = category.File.CreateImage(_env, "assets", "images");
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Ust Category Mutleq Secilmelidir");
                    return View(category);
                }

                if (!await _context.Categories.AnyAsync(c=>c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("ParentId", "Duzgun Ust Category Sec");
                    return View(category);
                }

                category.Image = null;
            }

            category.IsDeleted = false;
            category.CreatedAt = DateTime.UtcNow.AddHours(4);
            category.CreatedBy = "System";

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {

            if (id == null)
            {
                return BadRequest("Id Bos Ola Bilmez");
            }

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (category == null)
            {
                return NotFound("Daxil Edilen Id Yanlisir");
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsDeleted == false && c.IsMain)
                .ToListAsync();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id,Category category)
        {
            ViewBag.Categories = await _context.Categories
               .Where(c => c.IsDeleted == false && c.IsMain)
               .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            if (id == null)
            {
                return BadRequest("Id Bos Ola Bilmez");
            }

            Category existedCategory = await _context.Categories.FirstOrDefaultAsync(c => c.IsDeleted == false && c.Id == id);

            if (existedCategory == null)
            {
                return NotFound("Daxil Edilen Id Yanlisir");
            }

            if (category.Id != id)
            {
                return BadRequest("Id Bos Ola Bilmez");
            }

            if (category.IsMain)
            {
                if (category.File == null)
                {
                    ModelState.AddModelError("File", "Fayl Mecburidi");
                    return View(category);
                }

                if (category.File.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("File", "Fayl Tipi .jpg ve ya .jpeg olmalidir");
                    return View(category);
                }

                if ((category.File.Length / 1024) > 20)
                {
                    ModelState.AddModelError("File", "Fayl Olcusu maksimum 20 kb olmalidir");
                    return View(category);
                }

                existedCategory.Image = category.File.CreateImage(_env, "assets", "images");

                existedCategory.ParentId = null;
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Ust Category Mutleq Secilmelidir");
                    return View(category);
                }

                if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("ParentId", "Duzgun Ust Category Sec");
                    return View(category);
                }

                existedCategory.Image = null;
            }

            TempData["error"] = "Error Oldu";

            existedCategory.IsMain = category.IsMain;
            existedCategory.Name = category.Name;
            existedCategory.UpdatedAt = DateTime.UtcNow.AddHours(4);
            existedCategory.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
