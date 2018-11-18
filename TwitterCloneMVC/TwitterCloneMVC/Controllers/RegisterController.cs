using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using TwitterCloneMVC.Models;


namespace TwitterCloneMVC.Controllers
{
    public class RegisterController : Controller
    {
        dbTwitterEntities1 db = new dbTwitterEntities1();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult SaveData(People model)
        {
            model.active = false;

            db.People1.Add(model);
            db.SaveChanges();
            BuildEmailTemplate(model.user_id);
            return Json("Registeration Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Confirm(int regId)
        {
            ViewBag.regID = regId;
            return View();
        }

        public JsonResult RegisterConfirm(int regId)
        {
            People Data = db.People1.Where(x => x.user_id == regId).FirstOrDefault();
            Data.active = true;
            db.SaveChanges();
            var msg = "Your Email is verified";
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        private void BuildEmailTemplate(int regID)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "Text" + ".cshtml");
            var regInfo = db.People1.Where(x => x.user_id == regID).FirstOrDefault();
            var url = "http://localhost:54336/" + "Register/Confirm?regId=" + regID;
            body = body.Replace("@ViewBag.ConfirmationLink", url);
            body = body.ToString();
            BuildEmailTemplate("Your Account is Successfully Created", body, regInfo.email);

        }

        public static void BuildEmailTemplate(string subjectText, string bodyText, string sendTo)
        {
            string from, to, bcc, cc, subject, body;
            from = "example@gmail.com";
            to = sendTo.Trim();
            bcc = "";
            cc = "";
            subject = subjectText;
            StringBuilder sb = new StringBuilder();
            sb.Append(bodyText);
            body = sb.ToString();
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from);
            mail.To.Add(new MailAddress(to));
            if (!string.IsNullOrEmpty(bcc))
            {
                mail.Bcc.Add(new MailAddress(bcc));
            }
            if (!string.IsNullOrEmpty(bcc))
            {
                mail.CC.Add(new MailAddress(cc));
            }

            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            SendEmail(mail);

        }

        public static void SendEmail(MailMessage mail)
        {
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential("example@gmail.com", "password");
            try
            {
                client.Send(mail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult CheckValidUser(People model)
        {
            string result = "Fail";
            var Dataitem = db.People1.Where(x => x.email == model.email && x.password == model.password).SingleOrDefault();
            if (Dataitem != null)
            {
                Session["user_id"] = Dataitem.user_id.ToString();
                Session["UserName"] = Dataitem.fullName.ToString();
                result = "Success";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Afterlogin()
        {
            if (Session["user_id"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("index");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("index");
        }

    }
}