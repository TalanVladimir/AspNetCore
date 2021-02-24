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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<NotificationHub> _hubContext;

        public MembersController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IHubContext<NotificationHub> hubcontext)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubcontext;
        }

        // GET: Members
        public async Task<IActionResult> Index(string? searchString, string? currentFilter, int? pageNumber)
        {
            var members = from s in _context.Member
                          select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                foreach (string part in searchString.Split(' '))
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        if (!part.Contains(" "))
                        {
                            members = members.Where(s => s.confirmation_date.ToString().Contains(part)
                                                        || s.municipality_code.Contains(part)
                                                        || s.municipality_name.Contains(part)
                                                        || s.age_bracket.Contains(part)
                                                        || s.gender.Contains(part));
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
                    members = members.OrderByDescending(s => s.confirmation_date);
                    break;
            }
            int pageSize = 5;
            return View(await MembersPaginatedList<Member>.CreateAsync(members.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Members/Chart
        public async Task<IActionResult> Chart(string? groupBy)
        {
            string titleChart = "";
            string titleX = "";
            string[] setX = new string[0];
            string[] setY = new string[0];
            string getX = "";
            string getY = "";

            bool defView = true;
            switch (groupBy)
            {
                case "confirmation_date":
                    var query_confirmation_date = from s in _context.Member
                                                  orderby s.confirmation_date
                                                  group s by s.confirmation_date into g
                                                  select new
                                                  {
                                                      Date = g.Key,
                                                      Total = g.Count()
                                                  };
                    var res_confirmation_date = query_confirmation_date.OrderBy(s => s.Date);
                    foreach (var item in res_confirmation_date)
                    {
                        Array.Resize(ref setX, setX.Length + 1);
                        setX[setX.Length - 1] = item.Date.ToString();
                        Array.Resize(ref setY, setY.Length + 1);
                        setY[setY.Length - 1] = item.Total.ToString();
                    }
                    titleChart = "Group By Confirmation Date";
                    titleX = "Confirmation dates";
                    break;
                case "age_bracket":
                    var query_age_bracket = from s in _context.Member
                                            orderby s.age_bracket
                                            group s by s.age_bracket into g
                                            select new
                                            {
                                                Age = g.Key,
                                                Total = g.Count()
                                            };
                    var res_age_bracket = query_age_bracket.OrderBy(s => s.Age);
                    foreach (var item in res_age_bracket)
                    {
                        Array.Resize(ref setX, setX.Length + 1);
                        setX[setX.Length - 1] = item.Age;
                        Array.Resize(ref setY, setY.Length + 1);
                        setY[setY.Length - 1] = item.Total.ToString();
                    }
                    titleChart = "Group By Age Bracket";
                    titleX = "Age Brackets";
                    break;
                case "gender":
                    var query_gender = from s in _context.Member
                                       orderby s.gender
                                       group s by s.gender into g
                                       select new
                                       {
                                           Gender = g.Key,
                                           Total = g.Count()
                                       };
                    var res_gender = query_gender.OrderBy(s => s.Gender);
                    foreach (var item in res_gender)
                    {
                        Array.Resize(ref setX, setX.Length + 1);
                        setX[setX.Length - 1] = item.Gender;
                        Array.Resize(ref setY, setY.Length + 1);
                        setY[setY.Length - 1] = item.Total.ToString();
                    }
                    titleChart = "Group By Gender";
                    titleX = "Gender";
                    break;
                case "age_gender":
                    defView = false;
                    var query_age_gender = from r in _context.Member
                                           group r by new { r.age_bracket, r.gender } into g
                                           orderby g.Key.age_bracket
                                           select new { Age = g.Key.age_bracket, Gender = g.Key.gender, Count = g.Count() };

                    var all_gender = from s in _context.Member
                                     orderby s.gender
                                     group s by s.gender into g
                                     select new
                                     {
                                         Gender = g.Key,
                                         Count = g.Key.Count()
                                     };

                    int[] temp = new int[0];
                    string[] gender = new string[0];
                    string[] rez = new string[0];
                    foreach (var item in all_gender)
                    {
                        Array.Resize(ref gender, gender.Length + 1);
                        gender[gender.Length - 1] = item.Gender;
                        Array.Resize(ref temp, temp.Length + 1);
                        temp[temp.Length - 1] = 0;
                        Array.Resize(ref rez, rez.Length + 1);
                        rez[rez.Length - 1] = "";

                        titleX = titleX + "\"" + item.Gender + "\"";
                    }
                    int count = rez.Length;

                    string tempTitle = null;

                    int agCount = 0;
                    int vCount = query_age_gender.Count();
                    foreach (var item in query_age_gender)
                    {
                        ++agCount;
                        string tAge = item.Age;
                        string tGender = item.Gender;
                        int tCount = item.Count;
                        int ind = Array.IndexOf(gender, tGender);

                        if ((tempTitle != item.Age) && (agCount != 1))
                        {
                            Array.Resize(ref setX, setX.Length + 1);
                            setX[setX.Length - 1] = tAge;
                            for (int i = 0; i < count; i++)
                            {
                                if (string.IsNullOrEmpty(rez[i]))
                                    rez[i] = temp[i].ToString();
                                else
                                    rez[i] = rez[i] + "," + temp[i].ToString();
                                temp[i] = 0;
                            }
                        }
                        temp[ind] = temp[ind] + tCount;

                        if (agCount == vCount)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                rez[i] = rez[i] + "," + temp[i].ToString();
                                Array.Resize(ref setX, setX.Length + 1);
                                setX[setX.Length - 1] = tAge;
                            }
                        }
                        tempTitle = tAge;
                    }

                    string viewData = "series:[";
                    for (int i = 0; i < count; i++)
                    {
                        if (i != 0)
                        {
                            viewData = viewData + ",";
                        }
                        viewData = viewData + "{" + "name: '" + gender[i] + "',"
                                             + "data: [" + rez[i]
                                             + "]}";

                    }
                    viewData = viewData + "]";
                    titleChart = "Group By Age and Gender";

                    ViewData["series"] = viewData;
                    break;
                case "municipality_name":
                default:
                    var query_municipality_name = from s in _context.Member
                                                  orderby s.municipality_name
                                                  group s by s.municipality_name into g
                                                  select new
                                                  {
                                                      Name = g.Key,
                                                      Total = g.Count()
                                                  };
                    var res_municipality_name = query_municipality_name.OrderBy(s => s.Name);
                    foreach (var item in res_municipality_name)
                    {
                        Array.Resize(ref setX, setX.Length + 1);
                        setX[setX.Length - 1] = item.Name;
                        Array.Resize(ref setY, setY.Length + 1);
                        setY[setY.Length - 1] = item.Total.ToString();
                    }
                    titleChart = "Group By Municipality Name";
                    titleX = "Municipality Names";
                    break;
            }

            ViewData["getError"] = getY;
            getX = "['" + string.Join("','", setX) + "']";
            getY = "[" + string.Join(", ", setY) + "]";
            ViewData["getTitle"] = "'" + titleChart + "'";
            ViewData["getTitleX"] = "'" + titleX + "'";
            ViewData["getX"] = getX;
            ViewData["getY"] = getY;

            if (defView)
            {
                ViewData["series"] = "series:"
                                    + "[{"
                                    + "name:" + ViewData["getTitleX"] + ","
                                    + "data:" + ViewData["getY"]
                                    + "}]";
            }
            return View();
        }

        [Authorize]
        public IActionResult Database()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult DatabaseUpload(IFormFile? files)
        {
            using (_context) using (var transaction = _context.Database.BeginTransaction())
            {
                if (files != null)
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
            }
            return Redirect("Database");
        }

        [Authorize]
        public async Task<IActionResult> DatabaseClear(string? confirm)
        {
            if (!string.IsNullOrEmpty(confirm))
            {
                if (confirm.Contains("delete"))
                {
                    using (_context) using (var transaction = _context.Database.BeginTransaction())
                    {
                        _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TestMembers ON");
                        foreach (Member member in _context.Member)
                        {
                            _context.Member.Remove(member);
                        }
                        _context.SaveChanges();
                        _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TestMembers OFF");
                        transaction.Commit();
                    }
                    return Redirect("Database");
                }
            }
            return PartialView();
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
            return PartialView(member);
            //return View(member);
        }

        // GET: Members/Create
        [Authorize]
        public IActionResult Create()
        {
            return PartialView();
            //return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("OID,x,y,case_code,municipality_code,municipality_name,age_bracket,gender")] Member member)
        {
            if (ModelState.IsValid)
            {
                member.confirmation_date = DateTime.Now;
                _context.Add(member);
                await _context.SaveChangesAsync();
                await this._hubContext.Clients.All.SendAsync("ReceiveMessage", member.gender, member.confirmation_date.ToString());
                return RedirectToAction(nameof(Index));
            }
            return PartialView(member);
            //return View(member);
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
            return PartialView(member);
            //return View(member);
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

            return PartialView(member);
            //return View(member);
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
