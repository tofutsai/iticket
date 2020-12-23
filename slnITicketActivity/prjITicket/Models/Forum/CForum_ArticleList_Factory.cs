using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjITicket.Models.Forum
{
    public class CForum_ArticleList_Factory
    {
        //此類別為針對文章列表陳列的處理，包含討論版類別、跳頁、關鍵字搜尋、進階搜尋等等，所有方法都是輸入清單+條件、輸出清單。
        //輸出後請重新按照日期排列，組合時小心別重疊了
        //文章分類
        public List<Article> Article_Category(List<Article> articles, int Article_Category)
        {
            articles = articles.Where(n => n.ArticleCategoryID == Article_Category).ToList();
            return articles;
        }
        
        //關鍵字搜尋...for作者
        public List<Article> Article_Search_Editor(List<Article> articles, string Word)
        {
            articles = (from n in articles
                        where n.Member.NickName.Contains(Word)
                        select n).ToList();
            return articles;
        }
        //關鍵字搜尋...for文章標題
        public List<Article> Article_Search_Title(List<Article> articles, string Word)
        {
            articles = (from n in articles
                        where n.ArticleTitle.Contains(Word)
                        select n).ToList();
            return articles;
        }
        //關鍵字搜尋...for文章內容
        public List<Article> Article_Search_Content(List<Article> articles, string Word)
        {
            articles = (from n in articles
                        where n.ArticleContent.Contains(Word)
                        select n).ToList();
            return articles;
        }
        //按日期檢索
        public List<Article> Article_Search_Content(List<Article> articles, DateTime dateTime起, DateTime dateTime終)
        {
            articles = (from n in articles
                        where n.Date >= dateTime起 && n.Date <= dateTime終
                        select n).ToList();
            return articles;
        }
    }
}