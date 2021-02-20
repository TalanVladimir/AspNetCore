using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AspNetCore.Data;
using AspNetCore.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using LINQtoCSV;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.Controllers
{
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string? searchString, string? sortOrder, string? currentFilter, int? pageNumber)
        {
            //ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            //ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            var members = from s in _context.Member select s;

            IQueryable<string> membersGender = from m in _context.Member
                                               orderby m.confirmation_date
                                               select m.gender;

            if (!String.IsNullOrEmpty(searchString))
            {
                foreach (string part in searchString.Split(' '))
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        if (!part.Contains(" "))
                        {
                            members = members.Where(s => s.x.Contains(searchString)
                                                        || s.y.Contains(searchString)
                                                        || s.confirmation_date.ToString().Contains(searchString)
                                                        || s.municipality_code.Contains(searchString)
                                                        || s.municipality_name.Contains(searchString)
                                                        || s.age_bracket.Contains(searchString)
                                                        || s.gender.Contains(searchString));
                        }
                    }
                }
            }

            switch (currentFilter)
            {
                case "x":
                    members = members.OrderBy(s => s.x);
                    break;
                case "y":
                    members = members.OrderBy(s => s.y);
                    break;
                case "confirmation_date":
                    members = members.OrderBy(s => s.confirmation_date);
                    break;
                case "municipality_code":
                    members = members.OrderBy(s => s.municipality_code);
                    break;
                case "municipality_name":
                    members = members.OrderBy(s => s.municipality_name);
                    break;
                case "age_bracket":
                    members = members.OrderBy(s => s.age_bracket);
                    break;
                case "gender":
                    members = members.OrderBy(s => s.gender);
                    break;
                default:
                    members = members.OrderBy(s => s.confirmation_date);
                    break;
            }
            int pageSize = 500;
            return View(await MembersPaginatedList<Member>.CreateAsync(members.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Members/Chart
        public async Task<IActionResult> Chart()
        {
            return View(await _context.Member.ToListAsync());
        }

        [Authorize]
        public IActionResult DbUpload()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult DbUpload(IFormFile files)
        {
            using (_context) using (var transaction = _context.Database.BeginTransaction())
            {
                CsvFileDescription csvFileDescription = new CsvFileDescription
                {
                    SeparatorChar = ',',
                    FirstLineHasColumnNames = true
                };
                CsvContext csvContext = new CsvContext();
                StreamReader streamReader = new StreamReader(files.OpenReadStream());
                IEnumerable<Member> list = csvContext.Read<Member>(streamReader, csvFileDescription);

                //_context.Member.UpdateRange(list);
                _context.Member.AddRange(list);
                _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TestMembers ON");
                _context.SaveChanges();
                _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TestMembers OFF");
                transaction.Commit();
            }
            return Redirect("Members");
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.OID == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("OID,x,y,case_code,confirmation_date,municipality_code,municipality_name,age_bracket,gender")] Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("OID,x,y,case_code,confirmation_date,municipality_code,municipality_name,age_bracket,gender")] Member member)
        {
            if (id != member.OID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.OID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.OID == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        [Authorize]
        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Member.FindAsync(id);
            _context.Member.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Member.Any(e => e.OID == id);
        }
    }
}
