document.addEventListener('DOMContentLoaded', function () {
    $('#AjaxBoxDemo').on('click', function () {
        switch ($(this).data('demo')) {
            case 'Send':
                $('#AjaxBoxTextarea').val('感謝您長久以來對本站的支持與使用本站的服務, iTicket 祝您今天聖誕快樂!')
                break
            case 'Ban':
                $('#AjaxBoxTextarea').val('經相關人士舉證, 使用者冒用他人資料註冊本站會員')
                $('#AjaxBoxDate').val('2050-12-31')
                break
            case 'fPassFalse':
                $('#AjaxBoxTextarea').val('商家並未上傳相關審核必備資料')
                break
            case 'Delete':
                $('#AjaxBoxTextarea').val('您的文章違反本站的使用條款')
                break
            case 'DeleteBan':
                $('#AjaxBoxTextarea').val('您的文章違反本站的使用條款')
                $('#AjaxBoxTextarea2').val('使用者的文章嚴重違反本站規定')
                $('#AjaxBoxDate').val('2020-12-31')
                break
            case 'Hide':
                $('#AjaxBoxTextarea').val('您的評論違反本站的使用條款')
                break
            case 'HideBan':
                $('#AjaxBoxTextarea').val('您的評論違反本站的使用條款')
                $('#AjaxBoxTextarea2').val('使用者的評論嚴重違反本站規定')
                $('#AjaxBoxDate').val('2020-12-31')
                break
        }
    })
})