using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.Eventing.Reader;
using Tranning.DataDBContext;
using Tranning.Models;

namespace Tranning.Controllers
{
    public class TraineeCourseController : Controller
    {
        private readonly TranningDBContext _dbContext;
        public TraineeCourseController(TranningDBContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public IActionResult Index(string SearchString)
        {
            TraineeCourseModel traineecourseModel = new TraineeCourseModel();
            traineecourseModel.TraineeCourseDetailLists = new List<TraineeCourseDetail>();

            var data = from trainee_course in _dbContext.TraineeCourses
                       join users in _dbContext.Users on trainee_course.trainee_id equals users.id
                       join courses in _dbContext.Courses on trainee_course.course_id equals courses.id
                       select new { Trainee_Course = trainee_course, User = users, Course = courses };

            data = data.Where(m => m.Trainee_Course.deleted_at == null);
            if (!string.IsNullOrEmpty(SearchString))
            {
                data = data.Where(m => m.User.full_name.Contains(SearchString) || m.Course.name.Contains(SearchString));
            }
            data.ToList();

            foreach (var item in data)
            {
                // Use object initialization for better readability
                traineecourseModel.TraineeCourseDetailLists.Add(new TraineeCourseDetail
                {
                    trainee_id = item.Trainee_Course.trainee_id,
                    course_id = item.Trainee_Course.course_id,
                    created_at = item.Trainee_Course.created_at,

                    updated_at = item.Trainee_Course.updated_at,
                    full_name = item.User.full_name,
                    name = item.Course.name,
                });

            }

            ViewData["CurrentFilter"] = SearchString;
            return View(traineecourseModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            TraineeCourseDetail traineecourse = new TraineeCourseDetail();
            var courseList = _dbContext.Courses
              .Where(m => m.deleted_at == null)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = courseList;

            var traineeList = _dbContext.Users
              .Where(m => m.deleted_at == null && m.role_id == 4)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.full_name }).ToList();
            ViewBag.Stores1 = traineeList;

            return View(traineecourse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TraineeCourseDetail traineecourse)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var traineecourseData = new TraineeCourse()
                    {
                        course_id = traineecourse.course_id,
                        trainee_id = traineecourse.trainee_id,
                        created_at = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    };

                    _dbContext.TraineeCourses.Add(traineecourseData);
                    _dbContext.SaveChanges(true);
                    TempData["saveStatus"] = true;
                }
                catch (Exception ex)
                {

                    TempData["saveStatus"] = false;
                }
                return RedirectToAction(nameof(TraineeCourseController.Index), "TraineeCourse");
            }


            var courseList = _dbContext.Courses
              .Where(m => m.deleted_at == null)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.name }).ToList();
            ViewBag.Stores = courseList;

            var traineeList = _dbContext.Users
              .Where(m => m.deleted_at == null && m.role_id == 4)
              .Select(m => new SelectListItem { Value = m.id.ToString(), Text = m.full_name }).ToList();
            ViewBag.Stores1 = traineeList;


            Console.WriteLine(ModelState.IsValid);
            foreach (var key in ModelState.Keys)
            {
                var error = ModelState[key].Errors.FirstOrDefault();
                if (error != null)
                {
                    Console.WriteLine($"Error in {key}: {error.ErrorMessage}");
                }
            }
            return View(traineecourse);
        }

        [HttpGet]
        public IActionResult Delete(int id = 0)
        {
            try
            {
                var data = _dbContext.TraineeCourses.FirstOrDefault(m => m.course_id == id);

                if (data != null)
                {
                    // Soft delete by updating the deleted_at field
                    data.deleted_at = DateTime.Now;
                    _dbContext.SaveChanges();
                    TempData["DeleteStatus"] = true;
                }
                else
                {
                    TempData["DeleteStatus"] = false;
                }
            }
            catch (Exception ex)
            {
                TempData["DeleteStatus"] = false;
                // Log the exception if needed: _logger.LogError(ex, "An error occurred while deleting the topic.");
            }

            return RedirectToAction(nameof(Index), new { SearchString = "" });
        }

    }
}