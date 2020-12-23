let maxpage
const members = []
const emails = []

function AjaxMemberList() {
    let url
    switch (parseInt($('#fRoleId').val())) {
        case 0:
            url = '/BackEndMember/BanMemberList'
            break
        case 1:
        case 2:
        case 9:
            url = '/BackEndMember/GeneralList'
            break
        case 3:
            url = '/BackEndMember/MerchantList'
            break
        default:
            url = '/BackEndMember/GeneralList'
            break
    }
    $.ajax({
        url: url,
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
                let btnFile = `<button type="button" data-id="${e.MemberID}" data-email="${e.Email}" class="btn btn-info btn-sm text-white mr-2 customFile">資料</button>`
                let btnVerify = `<button type="button" data-id="${e.MemberID}" data-email="${e.Email}" class="btn btn-warning btn-sm text-white mr-2 customVerify">審核</button>`
                let btnBan = `<button type="button" data-id="${e.MemberID}" data-email="${e.Email}" class="btn btn-danger btn-sm mr-2 customBan">停權</button>`
                let btnUnBan = `<button type="button" data-id="${e.MemberID}" data-email="${e.Email}" class="btn btn-secondary btn-sm mr-2 customUnBan">解除</button>`
                let btnInfo = `<span>停權至 ${e.EndTime} <a href="javascript:" onclick="theReason('${e.Reason}')">停權原因</a></span>`
                $('#ListBody').append(`<tr class="${e.Reason === null ? 'text-body' : 'text-danger'}" style="background-color: ${members.includes(e.MemberID) ? 'lightyellow' : 'white'}">
                    <td data-id="${e.MemberID}" data-email="${e.Email}" class="pt-2 pb-1 text-body customSelectUser" style="cursor: pointer">${members.includes(e.MemberID) ? '<i class="far fa-check-square"></i>' : '<i class="far fa-square"></i>'}</td>
                    <td class="pt-2 pb-1"><a href="javascript:" onclick="theDetail(${e.MemberID})">${keyHighlight(e.Email.split('@', 2)[0], $('#fKeyword').val())}</a><br><sup class="text-secondary">&nbsp;&nbsp;&nbsp;@${e.Email.split('@', 2)[1]}</sup></td>
                    <td class="pt-2 pb-1">${keyHighlight(e.NickName, $('#fKeyword').val())}</td>
                    <td class="pt-2 pb-1">${keyHighlight(e.Name, $('#fKeyword').val())}</td>
                    <td class="pt-2 pb-1">${keyHighlight(e.Phone === null ? '' : e.Phone, $('#fKeyword').val())}</td>
                    <td class="pt-2 pb-1">${e.MemberRoleName}${e.Reason === null ? '' : '<span class="badge badge-danger ml-1">停權中</span>'}${e.MerchantNull === 'Null' ? '<span class="badge badge-warning text-white ml-1">未審核</span>' : ''}</td>
                    <td class="pt-2 pb-1">${e.MerchantNull === 'Null' ? btnFile + btnVerify : ''} ${e.MemberRoleId === 4 ? '' : e.Reason === null ? btnBan : btnUnBan + btnInfo}</td>
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

function theReason(reason) {
    swal('停權原因', reason, 'info')
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
                                rows3 += `<tr><td>${i+1}</td><td>${reasons[i]}</td><td>${endtimes[i]}</td></tr>`
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
    AjaxMemberList()
}

function theNext() {
    let pagecurrent = parseInt($('#fPageCurrent').val())
    $('#fPageCurrent').val(Math.min(pagecurrent + 1, maxpage))
    AjaxMemberList()
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
    $('#fSortRule').val("0")
    $('#pageAmount').prop('selectedIndex', 0)
    MemberRoleInfoFont($('#fSortRule').val())
    AjaxMemberList()
}

function MemberRoleInfoFont(value) {
    const fonts = [$('#ListHead1 span'), $('#ListHead2 span'), $('#ListHead3 span'), $('#ListHead4 span')]
    for (let font of fonts) {
        font.html('<i class="fas fa-sort"></i>').closest('th').css('color', 'black')
    }
    switch (value) {
        case '1a':
            fonts[0].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '1d':
            fonts[0].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '2a':
            fonts[1].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '2d':
            fonts[1].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '3a':
            fonts[2].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '3d':
            fonts[2].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '4a':
            fonts[3].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '4d':
            fonts[3].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        default:
            $('#ctrlBtn5').addClass('d-none')
            break
    }
}

function SendMessageTask(state, members, message) {
    $.ajax({
        url: '/BackEndMember/SendMessageAsync',
        type: 'post',
        data: {
            state: state,
            members: members,
            message: message
        },
        timeout: 20000,
        success: function (data) {
            swal('發送系統通知提示', data, 'success')
        },
        error: function (xmlhttprequest, textstatus, message) {
            swal('發送系統通知提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                button: false
            })
        }
    })
}