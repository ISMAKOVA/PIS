using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ANIMAUX.Models;
using ANIMAUX.Helpers;
using ANIMAUX.Controllers;
namespace ANIMAUX.Controllers
{
    public class HomeController : Controller
    {
        AnimauxEntities entities = new AnimauxEntities();
        
        public ActionResult Profile()
        {
            return View();
        }

        //////////////////////////РЕЕСТР///////////////////////////
        public IEnumerable<RegistryItems> RegistryItems()
        {
            Registry registry = new Registry();
            CurrentUser.setUser("Админ", 0, 0, 0);
            // CurrentUser.setUser("Куратор ВетСлужбы", 1, 1, 1); //id = 4
           // CurrentUser.setUser("Пользователь", 0, 2, 1);
            var user = CurrentUser.getUser();
            var userRole = user.role; //access_level
            IEnumerable<RegistryItems> registries = null;
            switch (userRole)
            {
                case 0:
                    registries = registry.GetLists();
                    break;
                case 1:
                    {
                        registry.GetListsByDistrict(user);
                        registries = registry.GetListsByOrganisation(user);
                        break;
                    }
            }
            return registries;
        }
        public ActionResult Registry()
        {
            FillDropDowns();
            ViewData["registryItems"] = RegistryItems();
            ViewBag.View = 0;
            return View(ViewData["registryItems"]);
        }
        [HttpPost]
        public ActionResult Registry(FormCollection form)
        {
            string date = form["dateInput"];
            string sex = form["dropDownSex"];
            char[] age = form["dropDownAge"].Where(c => Char.IsDigit(c)).ToArray();
            string district = form["dropDownDistrict"];
            string sort_id = form["dropDownSort"];

            ViewBag.View = Convert.ToInt32(form["view"]);
            var registryItems = Filter(RegistryItems(), date, sex, age, district);
            var result = Sort(registryItems, sort_id);        
            ViewData["registryItems"] = result;
            FillDropDowns();
            return View(ViewData["registryItems"]);
        }
        private IEnumerable<RegistryItems> Sort(IEnumerable<RegistryItems> registryItems, string sortOrder)
        {
            switch (sortOrder)
            {
                case "По возрастанию":
                    return registryItems.OrderBy(x => x.cards.id);
                case "По убыванию":
                    return registryItems.OrderByDescending(x => x.cards.id);
                default:
                    return registryItems;
            }
        }
        private IEnumerable<RegistryItems> Filter(IEnumerable<RegistryItems> registryItems, string date, string sex, char[] age, string district)
        {
            IEnumerable<RegistryItems> result = registryItems;
            if (date != "")
            {
                var convertedDate = Convert.ToDateTime(date);
                result = registryItems.Where(z => z.cards.date_added >= convertedDate);
                registryItems = result;
            }
            if (sex != "-1")
            {
                result = registryItems.Where(z => z.animals.sex.Equals(sex));
                registryItems = result;
            }
            if (age.Length != 1)
            {
                result = registryItems.Where(z => (DateTime.Now.Year - z.animals.birth_date.Year) >= Convert.ToInt32(Char.GetNumericValue(age[0])) && (DateTime.Now.Year - z.animals.birth_date.Year) < Convert.ToInt32(Char.GetNumericValue(age[1])));
                registryItems = result;
            }
            if (district != "-1")
            {
                result = registryItems.Where(z => z.districts.name.Equals(district));
                registryItems = result;
            }
            return registryItems;
        }
        public List<SelectListItem> CreateSelectList(List<string> list)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = list[0], Value = list[0], Selected = true });
            for (int i = 1; i < list.Count; i++)
            {
                items.Add(new SelectListItem { Text = list[i], Value = list[i].ToString() });
            }

            return items;
        }

        public void FillDropDowns()
        {
            ViewBag.DropDownSort = CreateSelectList(new List<string>() { "По умолчанию", "По возрастанию", "По убыванию" });
            ViewBag.DropDownSex = CreateSelectList(new List<string>() { "М", "Ж" });
            ViewBag.DropDownAge = CreateSelectList(new List<string>() { "0 - 1 года", "1 года - 2 лет", "2 - 4 года", "4 - 6 лет", "от 6 и старше" });
            ViewBag.DropDownDistrict = CreateSelectList(entities.districts.Select(z => z.name).Distinct().ToList());

        }

       

        ////////////////////////////КАРТА ЖИВОТНОГО////////////////////////////////
        public ActionResult Card(string cardId)
        {
            if (cardId != null)
            {
                ViewBag.cardId = cardId;
                int id = Convert.ToInt32(cardId);
                var card = entities.cards.Where(x => x.id == id).FirstOrDefault();
                var animal = entities.animals.Where(x => x.passport_number == card.animal_id).FirstOrDefault();
                var district = entities.districts.Where(x => x.id == card.district_id).FirstOrDefault();
                ViewBag.card = card;
                ViewBag.animal = animal;
                ViewBag.district = district;
            }
            else
            {
                ViewBag.cardId = "New";
                List<SelectListItem> animals = new List<SelectListItem>();
                var animalsdb = entities.animals;
                foreach (var item in animalsdb)
                {
                    animals.Add(new SelectListItem { Text = String.Format("{0} | {1} | {2}", item.passport_number, item.name, item.birth_date.ToShortDateString()), Value = item.passport_number.ToString() });
                }
                ViewBag.animals = animals;

                List<SelectListItem> districts = new List<SelectListItem>();
                var districtsdb = entities.districts;
                foreach (var item in districtsdb)
                {
                    districts.Add(new SelectListItem { Text = String.Format("{0} | {1}", item.id, item.name), Value = item.id.ToString() });
                }

                ViewBag.districts = districts;

                List<SelectListItem> organisations = new List<SelectListItem>();
                var organisationsdb = entities.organisations;
                foreach (var item in organisationsdb)
                {
                    organisations.Add(new SelectListItem { Text = String.Format("{0} | {1}", item.id, item.name), Value = item.id.ToString() });
                }

                ViewBag.organisations = organisations;
            }
            return View();
        }

        [HttpPost]
        public ActionResult DeleteCards(FormCollection form)
        {
            var card_id = Convert.ToInt32(form["cardId"]);
            if (card_id != -1)
            {
                cards card = new cards { id = card_id };

                entities.cards.Attach(card);
                entities.cards.Remove(card);
                entities.SaveChanges();
            }
            else
            {
                entities.Database.ExecuteSqlCommand("DELETE FROM[cards]");
            }
            return Redirect(Url.Action("Registry", "Home"));
        }

        [HttpPost]
        public ActionResult AddCard(FormCollection form)
        {
            var animal_id = Convert.ToInt32(form["dropDownAnimals"]);
            var district_id = Convert.ToInt32(form["dropDownDistricts"]);
            var organisation_id = Convert.ToInt32(form["dropDownOrganisations"]);
            var date_added = Convert.ToDateTime(form["date_added"]);

            cards card = new cards
            {
                organisation_id = organisation_id,
                district_id = district_id,
                animal_id = animal_id,
                date_added = date_added
            };

            entities.cards.Add(card);
            entities.SaveChanges();
            return Redirect(Url.Action("Registry", "Home"));
        }






        ////////////////////////////ОБЪЯВЛЕНИЯ////////////////////////////////
        public ActionResult Publications()
        {
            // ViewBag.Message = "Объявления";

            return View(entities.publications.ToList());
        }
    }

}