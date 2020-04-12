using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    [TestFixture]
    public class IviTests
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private string baseURL;

        [SetUp]
        public void SetupTest()
        {
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            baseURL = "https://www.google.com/";

            //неавторизованный пользователь заходит в https://www.google.com/
            driver.Navigate().GoToUrl(baseURL);

            //ищет ivi
            driver.FindElement(By.XPath("//input[@name='q']")).SendKeys("ivi");
            driver.FindElement(By.XPath("//input[@name='q']")).Submit();
        }

        [TearDown]
        public void TeardownTest()
        {
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
                // Игнорировать ошибки
            }            
        }

        [Test]
        public void PictureLinkTest()
        {            
            //переходит в картинки
            driver.FindElement(By.XPath("//a[.='Картинки']")).Click();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            //выбирает большие
            wait.Until(x => x.FindElement(By.XPath("//*[.='Инструменты']"))).Click();
            wait.Until(x => x.FindElement(By.XPath("//div[@jsname='Hxkmie']/div[1]/div[1]"))).Click();
            wait.Until(x => x.FindElement(By.XPath("//*[.='Большой']"))).Click();

            //убеждается, что не менее 3 картинок в выдаче ведут на сайт ivi.ru
            var allImages = driver.FindElements(By.XPath("//*[@jsname='uy6ald']"));
            int counter = 0;
            foreach(var image in allImages)
            {
                var link = image.GetAttribute("href").ToString();
                if (link.Contains("ivi.ru"))
                {
                    counter++;
                    if (counter == 3)
                        break;
                }
            }
            Assert.AreEqual(3, counter);
        }

        [Test]
        public void AppRatingTest()
        {            
            //на первых 5 страницах находит ссылки на приложение ivi в play.google.com
            string searchRating=null;

            for (int i = 1; i < 6;)
            {
                if (IsElementPresent(By.XPath("//*[contains(text(), 'play.google.com')]")))                
                {
                    var captions = driver.FindElement(By.XPath("//div[contains(text(),'Рейтинг')]")).Text.Split(' ');                    
                    searchRating = captions[1];
                    driver.FindElement(By.XPath("//*[contains(text(),'play.google.com')]")).Click();
                    break;
                }
                i++;
                
                var next = "//a[@aria-label='Page " + i+"']";
                driver.FindElement(By.XPath(next)).Click();
            }
            Assert.IsNotNull(searchRating);

            //убеждается, что рейтинг приложения на кратком контенте страницы совпадает с рейтингом при переходе
            var rating = driver.FindElement(By.XPath("//*[contains(@class,'BHMmbe')]"));           
            var marketRating = rating.Text;
            Assert.AreEqual(searchRating, marketRating);
        }

        [Test]
        public void WikiTest()
        {
            //на первых 5 страницах находит ссылку на статью в wikipedia об ivi
            var isFinded = false;
            for (int i = 1; i < 6;)
            {
                if (IsElementPresent(By.XPath("//*[contains(text(), 'wikipedia.org')]")))
                {
                    driver.FindElement(By.XPath("//*[contains(text(), 'wikipedia.org')]")).Click();
                    isFinded = true;
                    break;
                }
                i++;

                var next = "//a[@aria-label='Page " + i + "']";
                driver.FindElement(By.XPath(next)).Click();
            }
            Assert.IsTrue(isFinded);

            //убеждается, что в статье есть ссылка на официальный сайт ivi.ru
            var links = driver.FindElements(By.XPath("//a[@href='https://www.ivi.ru']"));
            Assert.GreaterOrEqual(links.Count, 1);
        }

        private bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}
