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
        public async Task<IActionResult> Index(string? searchString, string? currentFilter, int? pageNumber)
        {
            var members = from s in _context.Member select s;

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
            int pageSize = 100;
            return View(await MembersPaginatedList<Member>.CreateAsync(members.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Members/Chart
        public async Task<IActionResult> Chart(string? sortBy)
        {
            var members = from s in _context.Member select s;

            string[] setX = new string[0];

            int[] setRes0 = new int[0];
            int[] setRes1 = new int[0];
            int[] setRes2 = new int[0];

            int ind = 0;

            string getX = "";
            string getRes0 = "";
            string getRes1 = "";
            string getRes2 = "";
            string getTitle = "";

            switch (sortBy)
            {
                case "confirmation_date":
                    foreach (var item in members)
                    {
                        members = members.GroupBy(s => s.confirmation_date).Select(g => g.OrderByDescending(p => p.confirmation_date).FirstOrDefault());
                        string munName = item.confirmation_date.ToString();
                        ind = Array.IndexOf(setX, item.confirmation_date);
                        if (ind == -1)
                        {
                            Array.Resize(ref setX, setX.Length + 1);
                            setX[setX.Length - 1] = item.age_bracket;
                            Array.Resize(ref setRes0, setRes0.Length + 1);
                            setRes0[setRes0.Length - 1] = 0;
                            Array.Resize(ref setRes1, setRes1.Length + 1);
                            setRes1[setRes1.Length - 1] = 0;
                            Array.Resize(ref setRes2, setRes2.Length + 1);
                            setRes2[setRes2.Length - 1] = 0;
                            ind = setX.Length - 1;
                        }

                        switch (item.gender)
                        {
                            case "Vyras":
                                ++setRes0[ind];
                                break;
                            case "Moteris":
                                ++setRes1[ind];
                                break;
                            case "nenustatyta":
                            default:
                                ++setRes2[ind];
                                break;
                        }

                        getTitle = "Sort By confirmation Date";

                        getX = "['" + string.Join("','", setX) + "']";
                        getRes0 = "[" + string.Join(", ", setRes0) + "]";
                        getRes1 = "[" + string.Join(", ", setRes1) + "]";
                        getRes2 = "[" + string.Join(", ", setRes2) + "]";
                    }

                    members = members.OrderBy(s => s.confirmation_date);
                    break;
                case "age_bracket":
                    foreach (var item in members)
                    {
                        string munName = "\"" + item.age_bracket + "\"";
                        ind = Array.IndexOf(setX, item.age_bracket);
                        if (ind == -1)
                        {
                            Array.Resize(ref setX, setX.Length + 1);
                            setX[setX.Length - 1] = item.age_bracket;
                            Array.Resize(ref setRes0, setRes0.Length + 1);
                            setRes0[setRes0.Length - 1] = 0;
                            Array.Resize(ref setRes1, setRes1.Length + 1);
                            setRes1[setRes1.Length - 1] = 0;
                            Array.Resize(ref setRes2, setRes2.Length + 1);
                            setRes2[setRes2.Length - 1] = 0;
                            ind = setX.Length - 1;
                        }

                        switch (item.gender)
                        {
                            case "Vyras":
                                ++setRes0[ind];
                                break;
                            case "Moteris":
                                ++setRes1[ind];
                                break;
                            case "nenustatyta":
                            default:
                                ++setRes2[ind];
                                break;
                        }

                        getTitle = "Sort By Age Bracket";

                        getX = "['" + string.Join("','", setX) + "']";
                        getRes0 = "[" + string.Join(", ", setRes0) + "]";
                        getRes1 = "[" + string.Join(", ", setRes1) + "]";
                        getRes2 = "[" + string.Join(", ", setRes2) + "]";
                    }

                    members = members.OrderBy(s => s.age_bracket);
                    break;
                case "municipality_name":
                default:
                    foreach (var item in members)
                    {
                        string munName = "\"" + item.municipality_name + "\"";
                        ind = Array.IndexOf(setX, item.municipality_name);
                        if (ind == -1)
                        {
                            Array.Resize(ref setX, setX.Length + 1);
                            setX[setX.Length - 1] = item.municipality_name;
                            Array.Resize(ref setRes0, setRes0.Length + 1);
                            setRes0[setRes0.Length - 1] = 0;
                            Array.Resize(ref setRes1, setRes1.Length + 1);
                            setRes1[setRes1.Length - 1] = 0;
                            Array.Resize(ref setRes2, setRes2.Length + 1);
                            setRes2[setRes2.Length - 1] = 0;
                            ind = setX.Length - 1;
                        }

                        switch (item.gender)
                        {
                            case "Vyras":
                                ++setRes0[ind];
                                break;
                            case "Moteris":
                                ++setRes1[ind];
                                break;
                            case "nenustatyta":
                            default:
                                ++setRes2[ind];
                                break;
                        }

                        getTitle = "Sort By Municipality Name";

                        getX = "['" + string.Join("','", setX) + "']";
                        getRes0 = "[" + string.Join(", ", setRes0) + "]";
                        getRes1 = "[" + string.Join(", ", setRes1) + "]";
                        getRes2 = "[" + string.Join(", ", setRes2) + "]";
                    }

                    members = members.OrderBy(s => s.municipality_name);
                    break;
            }
            ViewData["getTitle"] = "'" + getTitle + "'";
            ViewData["getX"] = getX;
            ViewData["getRes0"] = getRes0;
            ViewData["getRes1"] = getRes1;
            ViewData["getRes2"] = getRes2;

            //return View(await _context.Member.ToListAsync());
            return View();
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
