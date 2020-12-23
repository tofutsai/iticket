using Newtonsoft.Json;
using prjITicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.ViewModel.BackEnd
{
    public class CBackEndMainArticle
    {
        [JsonIgnore]
        public Article Article { get; set; }
        public string ArticleTitle { get { return this.Article.ArticleTitle; } }
        public string ArticleContent { get { return this.Article.ArticleContent; } }
        public DateTime Date { get { return this.Article.Date; } }
        public string Picture { get { return this.Article.Picture; } }

        public string ArticleCategoryName { get; set; }
        public string MemberName { get; set; }

    }
}