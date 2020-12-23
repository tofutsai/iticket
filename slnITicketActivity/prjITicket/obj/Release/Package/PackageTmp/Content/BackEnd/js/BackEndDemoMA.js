document.addEventListener('DOMContentLoaded', function () {
    $('#AjaxBoxDemo').on('click', function () {
        switch ($(this).data('demo')) {
            case 'Send':
                $('#AjaxBoxTextarea').val('iTicket 祝您聖誕快樂')
                break
            case 'Ban':
                $('#AjaxBoxTextarea').val('使用者未填寫真實會員資料')
                $('#AjaxBoxDate').val('2020-12-31')
                break
            case 'Delete':
                $('#AjaxBoxTextarea').val('您的文章違反本站的使用條款')
                break
            case 'DeleteBan':
                $('#AjaxBoxTextarea').val('您的文章違反本站的使用條款')
                $('#AjaxBoxTextarea2').val('使用者的文章嚴重違反本站規定')
                $('#AjaxBoxDate').val('2020-12-31')
                break
        }
    })
})