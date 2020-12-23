$('#AjaxBoxDemo').on('click', function () {
    switch (location.pathname) {
        case '/BackEndComment/CommentList':
            $('#BanTaskMessage').val('非常抱歉, iTicket 必須隱藏您的評論, 因為您的評論違反本站的使用條款.')
            break
        case '/BackEndArticle/ArticleList':
            $('#BanTaskMessage').val('非常抱歉, iTicket 必須刪除您的文章, 因為您的文章違反本站的使用條款.')
            break
        case '/BackEndMember/MemberList':
            $('#BanTaskMessage').val('由於使用者違規情形頻繁, iTicket 管理員決定停權您的會員權限!')
            break
    }
})