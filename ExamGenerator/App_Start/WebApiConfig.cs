using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ExamGenerator
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Usuń znaczniki komentarza poniższego wiersza kodu, aby włączyć obsługę zapytań dla akcji z typem zwracanym IQueryable lub IQueryable<T>.
            // Aby uniknąć przetwarzania nieoczekiwanych lub złośliwych zapytań, weryfikuj zapytania przychodzące za pomocą ustawień weryfikacji w atrybucie QueryableAttribute.
            // Aby uzyskać więcej informacji, odwiedź stronę http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
        }
    }
}