let maxpage

function AjaxArticleList() {
    $('#ArticleManagement').addClass('d-none')
    $.ajax({
        url: $('#fType').val() === 'R' ? '/BackEndArticle/ReplyList' : '/BackEndArticle/ArticleList',
        type: 'post',
        data: $('#ctrlForm').serialize(),
        success: function (data) {
            if (data.length && data[0].ChangePage != 0) {
                $('#fPageCurrent').val(data[0].ChangePage)
            }
            maxpage = data.length ? Math.ceil(data[0].Count / parseInt($('#fPageSize').val())) : 1
            $('#ctrlHint').text($('#fKeyword').val() == '' ? '' : `關鍵字: ${$('#fKeyword').val()}`)
            $('#ListBody').empty()
            $.each(data, function (i, e) {
                $('#ListBody').append(`<tr data-id="${e.ARID}" style="cursor: pointer">
                    <td class="pt-2 pb-1"><img src="${e.Picture}" class="img-fluid img-thumbnail" onerror="this.src='https://via.placeholder.com/600x200?text=ITicket'"></td>
                    <td class="pt-2 pb-1 font-weight-bold">${keyHighlight(e.Title, $('#fKeyword').val())}</td>
                    <td class="pt-2 pb-1">${keyHighlight(e.Author.split('@', 2)[0], $('#fKeyword').val())}<br><sup class="text-secondary">&nbsp;&nbsp;&nbsp;@${e.Author.split('@', 2)[1]}</sup></td>
                    <td class="pt-2 pb-1">${e.CategoryName}</td>
                    <td class="pt-2 pb-1">${e.Date.split(' ')[0]}&nbsp;&nbsp;&nbsp;${e.Date.split(' ')[1]}</td>
                    <td class="pt-2 pb-1">${e.ReportCount}</td>
                </tr>`)
            })
            let pagecurrent = parseInt($('#fPageCurrent').val())
            $(`#pageTop li`).not('.default').remove().end().removeClass('disabled')
            $(`#pageBottom li`).not('.default').remove().end().removeClass('disabled')
            if (pagecurrent === 1) {
                $(`#pageTop li`).first().addClass('disabled')
                $(`#pageBottom li`).first().addClass('disabled')
            }
            if (pagecurrent === maxpage) {
                $(`#pageTop li`).last().addClass('disabled')
                $(`#pageBottom li`).last().addClass('disabled')
            }
            let showpage = [1, 2, pagecurrent - 2, pagecurrent - 1, pagecurrent, pagecurrent + 1, pagecurrent + 2, maxpage - 1, maxpage]
            let pointer1 = $(`#pageTop li`).first()
            let pointer2 = $(`#pageBottom li`).first()
            let flag = false
            for (let i = 1; i <= maxpage; i++) {
                if (!showpage.includes(i) && flag) {
                    continue
                }
                if (!showpage.includes(i)) {
                    pointer1.after(
                        `<li class="page-item"><a class="page-link text-secondary customGotoNewPage" href="javascript:">…</a></li>`
                    )
                    pointer1 = pointer1.next()
                    pointer2.after(
                        `<li class="page-item"><a class="page-link text-secondary customGotoNewPage" href="javascript:">…</a></li>`
                    )
                    pointer2 = pointer2.next()
                    flag = true
                    continue
                }
                pointer1.after(
                    `<li class="page-item"><a data-id="${i}" class="page-link customGotoPage" href="javascript:" style="background-color: ${pagecurrent === i ? 'lightyellow' : 'transparent'}">${i}</a></li>`
                )
                pointer1 = pointer1.next()
                pointer2.after(
                    `<li class="page-item"><a data-id="${i}" class="page-link customGotoPage" href="javascript:" style="background-color: ${pagecurrent === i ? 'lightyellow' : 'transparent'}">${i}</a></li>`
                )
                pointer2 = pointer2.next()
                flag = false
            }
            let begin = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + 1
            let ending = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + data.length
            $('#pageMessage').text(begin <= ending ? `顯示第 ${begin} 筆到第 ${ending} 筆資料` : `沒有符合的資料`)
            if ($(window).scrollTop() > 225) {
                $(window).scrollTop(225)
            }
        }
    })
}

function theDetail(id) {
    const displayName = {
        'SplitLine1': '------------',
        'Email': 'Email',
        'Name': '姓名',
        'IDentityNumber': '身分證字號',
        'Passport': '護照',
        'NickName': '暱稱',
        'BirthDate': '生日',
        'Phone': '電話',
        'Point': '獎勵點數',
        'Address': '地址',
        'MemberRoleName': '角色權限',
        'Sex': '性別',
        'District': '城市',
        'SplitLine2': '------------',
        'CompanyName': '公司名',
        'TaxIDNumber': '統編',
        'SellerHomePage': '商家網站主頁',
        'SellerPhone': '商家聯絡資訊',
        'SellerDiscription': '商家描述資訊',
        'fPass': '審核狀態'
    }
    $.ajax({
        url: '/BackEndMember/MemberDetail',
        type: 'post',
        data: { id: id },
        success: function (data) {
            $('#seller-tab').removeClass('d-none')
            $('#member-tab').tab('show')
            let part = 3
            let reasons
            let endtimes
            let rows1 = ''
            let rows2 = ''
            let rows3 = ''
            for (let field in data) {
                if (part === 3) {
                    if (field === 'Reasons') {
                        reasons = data[field]
                    }
                    if (field === 'EndTimes') {
                        endtimes = data[field]
                    }
                    if (field === 'SplitLine1') {
                        if (reasons.length) {
                            for (let i = 0; i < reasons.length; i++) {
                                rows3 += `<tr><td>${i + 1}</td><td>${reasons[i]}</td><td>${endtimes[i]}</td></tr>`
                            }
                            let html3 = (`
                                <div class="table-responsive text-body bg-white">
                                    <table class="table table-bordered table-striped mb-0" style="width: 100%;">
                                        <tr><th>#</th><th>停權原因</th><th>結束時間</th></tr>
                                        ${rows3}
                                    <table>
                                </div>
                            `)
                            $('#banlist').html(html3)
                        } else {
                            $('#banlist').html(`
                                <div class="table-responsive text-body bg-white">
                                    <table class="table table-bordered table-striped mb-0" style="width: 100%;">
                                        <tr><th>#</th><th>停權原因</th><th>結束時間</th></tr>
                                        <tr><td colspan="3">此會員沒有停權紀錄</td></tr>
                                    <table>
                                </div>
                            `)
                        }
                        part = 1
                        continue
                    }
                }
                if (part === 1) {
                    if (field !== 'SplitLine2') {
                        rows1 += `<tr><th>${displayName[field]}</th>
                            <td>${data[field] === null || data[field] === '' ? '<span class="text-danger">未填寫</span>' : data[field]}</td></tr>`
                    } else {
                        part = 2
                        continue
                    }
                }
                if (part === 2) {
                    if (field !== 'fPass') {
                        rows2 += `<tr><th>${displayName[field]}</th>
                            <td>${data[field] === null || data[field] === '' ? '<span class="text-danger">未填寫</span>' : data[field]}</td></tr>`
                    } else {
                        let fPassClass
                        switch (data['fPass']) {
                            case '審核通過':
                                fPassClass = 'text-success'
                                break
                            case '審核不通過':
                                fPassClass = 'text-danger'
                                break
                            default:
                                fPassClass = 'text-warning'
                                break
                        }
                        rows2 += `<tr><th>${displayName[field]}</th><td class="${fPassClass}">${data[field]}</td></tr>`
                    }
                }
            }
            let html1 = (`
                <div class="table-responsive text-body bg-white">
                    <table class="table table-bordered table-striped mb-0" style="width: 100%;">
                        ${rows1}
                    <table>
                </div>
            `)
            $('#member').html(html1)
            let html2 = (`
                <div class="table-responsive text-body bg-white">
                    <table class="table table-bordered table-striped mb-0" style="width: 100%;">
                        ${rows2}
                    <table>
                </div>
            `)
            $('#seller').html(html2)
            if (part === 1) {
                $('#seller-tab').addClass('d-none')
            }
            $('#AjaxDetail').modal({ backdrop: 'static', keyboard: false, show: true })
            $('#DK').off('click').one('click', function () {
                $(this).prev().click()
            })
        }
    })
}

function thePrev() {
    let pagecurrent = parseInt($('#fPageCurrent').val())
    $('#fPageCurrent').val(Math.max(1, pagecurrent - 1))
    AjaxArticleList()
}

function theNext() {
    let pagecurrent = parseInt($('#fPageCurrent').val())
    $('#fPageCurrent').val(Math.min(pagecurrent + 1, maxpage))
    AjaxArticleList()
}

function keyHighlight(text, keyword) {
    const prefix = []
    const suffix = []
    for (let i = 0; i < text.length; i++) {
        if (text.toLowerCase().slice(i).search(keyword) === 0) {
            prefix.push(i)
            suffix.push(i + keyword.length)
        }
    }
    while (prefix.length) {
        let iS = suffix.pop()
        text = `${text.slice(0, iS)}</span>${text.slice(iS)}`
        let iP = prefix.pop()
        text = `${text.slice(0, iP)}<span style="background-color: springgreen">${text.slice(iP)}`
    }
    return text
}

function keyInput() {
    $('#fKeyword').val($('#searchbox').val().toLowerCase().trim())
    $('#fPageCurrent').val(1)
    $('#fPageSize').val(10)
    $('#pageAmount').prop('selectedIndex', 0)
    AjaxArticleList()
}