using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P228Allup.DAL;
using P228Allup.Extension;
using P228Allup.Helpers;
using P228Allup.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P228Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products.Include(c => c.Category).Include(b => b.Brand).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Category =await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            ViewBag.Brands =await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Category = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            if(product.MainImageFile == null)
            {
                ModelState.AddModelError("MainImageFile", "Sekil olmalidi");
                return View(product);
            }

            if (product.HoverImageFile == null)
            {
                ModelState.AddModelError("HoverImageFile", "Sekil olmalidi");
                return View(product);
            }

            if(product.ProductImagesFile.Count() > 10)
            {
                ModelState.AddModelError("ProductImagesFile", "10 sekilden artiq yuklemek olmaz");
                return View(product);
            }

            if(product.ProductImagesFile.Count() > 0 || product.ProductImagesFile != null)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile formFile in product.ProductImagesFile)
                {
                    if(formFile != null)
                    {
                        ProductImage productImage = new ProductImage
                        {
                            Name = formFile.CreateImage(_env, "assets", "images", "product"),
                            CreatedAt = DateTime.UtcNow.AddHours(4)
                        };
                        productImages.Add(productImage);
                    }
                }

                product.ProductImages = productImages;
            }
            else
            {
                ModelState.AddModelError("ProductImagesFile", "Sekil olmalidi");
                return View(product);
            }

            if (!product.MainImageFile.CheckFileSize(1000))
            {
                ModelState.AddModelError("", "Sekil maximum 1000kb olmalidi");
                return View(product);
            }


            product.MainImage = product.MainImageFile.CreateImage(_env, "assets", "images", "product");
            product.HoverImage = product.HoverImageFile.CreateImage(_env, "assets", "images", "product");


            if(product.TagIds.Count() > 0 || product.TagIds != null)
            {
                List<ProductTag> productTags = new List<ProductTag>();

                for (int i = 0; i < product.TagIds.Count ; i++)
                {
                    ProductTag productTag = new ProductTag
                    {
                        TagId = product.TagIds[i],
                        CreatedAt = DateTime.UtcNow.AddHours(4)
                    };
                    productTags.Add(productTag);
                }
                product.ProductTags = productTags;
            }
            else
            {
                ModelState.AddModelError("ProductTags", "Tag mutleq secilmelidir");
                return View(product);
            }

            if(!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Category sef secilib");
                return View(product);
            }
            if (!await _context.Brands.AnyAsync(c => c.Id == product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Brand sef secilib");
                return View(product);
            }

            if(product.Count <= 0)
            {
                ModelState.AddModelError("Count", "Count sef daxil edilib");
                return View();
            }

            product.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Category = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();

            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(c => c.Category)
                .Include(b => b.Brand)
                .Include(t => t.ProductTags)
                .ThenInclude(tp => tp.Tag)
                .Include(pi => pi.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();


            product.TagIds = _context.ProductTags.Select(pt => pt.Id).ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product)
        {
            ViewBag.Category = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();

            if (id == null) return BadRequest();
            if (id != product.Id) return BadRequest();

            Product dbproduct = await _context.Products
                .Include(c => c.Category)
                .Include(b => b.Brand)
                .Include(t => t.ProductTags)
                .ThenInclude(tp => tp.Tag)
                .Include(pi => pi.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (dbproduct == null) return NotFound();



            if (!ModelState.IsValid)
            {
                return View(dbproduct);
            }

            foreach (int tagId in product.TagIds)
            {
                if(!await _context.Tags.AnyAsync(t => t.Id == tagId))
                {
                    ModelState.AddModelError("TagIds", "Tag duzgun secilmiyib");
                    return View(dbproduct);
                }
            }

            if(product.TagIds.Count() > 0)
            {
                _context.ProductTags.RemoveRange(dbproduct.ProductTags);
                List<ProductTag> productTags = new List<ProductTag>();
                for (int i = 0; i < product.TagIds.Count; i++)
                {
                    ProductTag productTag = new ProductTag
                    {
                        TagId = product.TagIds[i],
                        UpdatedAt = DateTime.UtcNow.AddHours(4)
                    };
                    productTags.Add(productTag);
                }
                product.ProductTags = productTags;
                dbproduct.ProductTags = product.ProductTags;
            }
            else
            {
                ModelState.AddModelError("TagIds", "Tag mutleq secilmelidir");
                return View(dbproduct);
            }

            if(product.MainImageFile != null)
            {
                Helper.DeleteFile(_env, dbproduct.MainImage,"assets","images","product");
                dbproduct.MainImage = product.MainImageFile.CreateImage(_env, "assets","images","product");
            }

            if (product.HoverImageFile != null)
            {
                Helper.DeleteFile(_env, dbproduct.HoverImage, "assets", "images", "product");
                dbproduct.HoverImage = product.HoverImageFile.CreateImage(_env, "assets", "images", "product");
            }

            dbproduct.Price = product.Price;
            dbproduct.Title = product.Title;
            dbproduct.DiscountedPrice = product.DiscountedPrice;
            dbproduct.IsBestSeller = product.IsBestSeller;
            dbproduct.IsFeatured = product.IsFeatured;
            dbproduct.UpdatedAt = DateTime.UtcNow.AddHours(4);
            dbproduct.CategoryId = product.CategoryId;
            dbproduct.BrandId = product.BrandId;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.Include(pi => pi.ProductImages).FirstOrDefaultAsync(p => p.ProductImages.Any(pi => pi.Id == id && !pi.IsDeleted));

            if (product == null) return BadRequest();

            ProductImage proimg = product.ProductImages.FirstOrDefault(p => p.Id == id);

            Helper.DeleteFile(_env, proimg.Name, "dist", "images", "products");

            product.ProductImages.FirstOrDefault(p => p.Id == id).IsDeleted = true;
            product.ProductImages.FirstOrDefault(p => p.Id == id).DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return PartialView("_ProductDeleteImages", product.ProductImages.Where(p => !p.IsDeleted));
        }
    }
}
