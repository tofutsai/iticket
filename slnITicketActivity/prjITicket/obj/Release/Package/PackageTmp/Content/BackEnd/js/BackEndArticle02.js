document.addEventListener('DOMContentLoaded', function () {
    // DOMContentLoaded
    AjaxArticleList()
    $('#ctrlBtnA').addClass('btn-outline-warning')

    // Sticky - Navbar
    setInterval(() => {
        if ($(window).innerWidth() > 975 && !$('body').hasClass('sb-sidenav-toggled')) {
            $('#ctrlBtn').css({ transition: 'margin-left .1s ease-out', marginLeft: '0' })
        } else if ($(window).innerWidth() > 800) {
            $('#ctrlBtn').css({ transition: 'margin-left .1s ease-out', marginLeft: '225px' })
        } else {
            $('#ctrlBtn').css({ transition: 'margin-left .1s ease-out', marginLeft: '0' })
        }
    }, 10)

    // Sticky - Table
    $(window).scroll(() => {
        if ($(this).scrollTop() > 225) {
            $('#cardHeader').addClass('py-1')
            $('#ListHead tr th').css('top', '87px')
        } else if ($(this).scrollTop() < 215) {
            $('#cardHeader').removeClass('py-1')
            $('#ListHead tr th').css('top', '103px')
        }
    })

    // Overwrite - Bootstrap Button
    $(document).on('click', '.btn, .page-item', function () {
        $(this).css({
            'outline': 'none !important',
            'box-shadow': 'none'
        })
    }).on('mouseenter', '#ArticleManagementUrl', function () {
        $(this).addClass('text-danger')
    }).on('mouseleave', '#ArticleManagementUrl', function () {
        $(this).removeClass('text-danger')
    })

    // PageCurrent Change
    $('#pageTop, #pageBottom').on('click', '.customGotoPage', function () {
        $('#fPageCurrent').val(parseInt($(this).data('id')))
        AjaxArticleList()
    }).on('click', '.customGotoNewPage', function () {
        swal({
            title: '請輸入頁碼',
            content: 'input'
        }).then(page => {
            if (page !== null && /^(?=.*[1-9])[0-9]+$/.test(page)) {
                $('#fPageCurrent').val(Math.min(parseInt(page), maxpage))
                AjaxArticleList()
            }
        })
    })

    // PageSize Change
    $('#pageAmount').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val($(this).val())
        AjaxArticleList()
    })

    // Keyword Change
    $('#searchbox').on('input', keyInput).on('compositionstart', function () {
        $(this).off('input')
    }).on('compositionend', function () {
        keyInput()
        $(this).on('input', keyInput)
    })

    // ctrlCate / ctrlDate / ctrlReport Change
    $('#ctrlCate, #ctrlDate, #ctrlReport').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        AjaxArticleList()
    })

    // ctrlBtn - ctrlBtnA
    $('#ctrlBtnA').on('mouseenter', function () {
        $(this).addClass('btn-outline-warning')
    }).on('mouseleave', function () {
        if ($('#fType').val() !== 'A') {
            $(this).removeClass('btn-outline-warning')
        }
    }).on('click', function () {
        if ($('#fType').val() === 'A') {
            return
        }
        $(this).addClass('btn-outline-warning')
        $('#ctrlBtnR').removeClass('btn-outline-warning')
        $('#fType').val('A')
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        AjaxArticleList()
    })

    // ctrlBtn - ctrlBtnR
    $('#ctrlBtnR').on('mouseenter', function () {
        $(this).addClass('btn-outline-warning')
    }).on('mouseleave', function () {
        if ($('#fType').val() !== 'R') {
            $(this).removeClass('btn-outline-warning')
        }
    }).on('click', function () {
        if ($('#fType').val() === 'R') {
            return
        }
        $(this).addClass('btn-outline-warning')
        $('#ctrlBtnA').removeClass('btn-outline-warning')
        $('#fType').val('R')
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        AjaxArticleList()
    })

    // ctrlBtn - ctrlBtnC
    $('#ctrlBtnC').on('click', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#fAuthorSearch').val(0)
        $(this).addClass('d-none').html('')
        AjaxArticleList()
    })

    // ListBody
    $('#ListBody').on('mouseenter', 'tr:not(".ArticleDetail")', function () {
        $(this).addClass('bg-warning')
    }).on('mouseleave', 'tr:not(".ArticleDetail")', function () {
        $(this).removeClass('bg-warning')
    }).on('click', 'tr:not(".ArticleDetail")', function () {
        $('.ArticleDetail').remove()
        if ($(this).hasClass('currentDetail')) {
            $(this).css('background-color', 'white').removeClass('currentDetail')
            $('#ArticleManagement').addClass('d-none')
            return
        }
        $(this).css('background-color', '#fffdaf').addClass('currentDetail')
            .siblings().css('background-color', 'white').removeClass('currentDetail')
        $.ajax({
            url: $('#fType').val() === 'R' ? '/BackEndArticle/ReplyDetail' : '/BackEndArticle/ArticleDetail',
            type: 'post',
            data: {
                id: $(this).data('id')
            },
            success: (data) => {
                $('#ArticleManagement').removeClass('d-none')
                $('#ArticleManagementUrl').text(data[0].ArticleTitle)
                    .attr('href', `/Forum/forum_content?articleID=${data[0].ArticleId}`)
                $('#ArticleManagementDelete').html($('#fType').val() === 'R' ?
                    '<i class="fas fa-trash-alt mr-1"></i>刪除回覆' : '<i class="fas fa-trash-alt mr-1"></i>刪除文章')
                    .data('id', `${data[0].MemberId}`).data('arid', `${$('#fType').val()}${data[0].ARID}`)
                $('#ArticleManagementDeleteBan').html($('#fType').val() === 'R' ?
                    '<i class="fas fa-ban mr-1"></i>刪除回覆並停權處分' : '<i class="fas fa-ban mr-1"></i>刪除文章並停權處分')
                    .data('id', `${data[0].MemberId}`).data('arid', `${$('#fType').val()}${data[0].ARID}`)
                $('#ArticleManagementSearch').data('id', `${data[0].MemberId}`).data('email', `${data[0].MemberEmail}`)
                $('#ArticleManagementFlag').html(`<i class="fas fa-flag mr-1"></i> 檢舉數: ${$(this).children().last().text()}`)
                $(this).after(`<tr class="ArticleDetail" style="background-color: lightyellow">
                    <td colspan="4" class="p-0" style="opacity: 0;">
                        <div class="preventLink"></div>
                    </td>
                    <td colspan="2" class="p-0" style="opacity: 0;">
                        <div id="ReportInfo"></div>
                    </td>
                </tr>`)
                $('.preventLink').html(`
                    <h4 class="px-3 py-2">使用者 
                        <a id="ArticleAuthorSearch" href="javascript: $('#ArticleManagementSearch').click()" class="text-info text-decoration-none">${data[0].MemberEmail}</a>
                        ${$('#fType').val() === 'R' ? '的回覆內容 :' : '的文章內容:'}
                    </h4>
                    <div id="ContentCard" class="card m-auto" style="width: 90%;">
                        <h5 class="card-header" style="background-color: #fffdaf;">${data[0].ArticleTitle}</h5>
                        <div class="card-body">${data[0].Content}</div>
                    </div>
                `)
                $('.preventLink a:not("#ArticleAuthorSearch")').on('click', function (e) {
                    e.preventDefault()
                    swal('系統提示', '是否要開啟外部連結?', 'warning', {
                        buttons: {
                            cancel: '不要',
                            confirm: '好啊'
                        }
                    }).then(x => {
                        if (x) {
                            window.open($(this).attr('href'))
                        }
                    })
                })
                $('.preventLink img').addClass('img-fluid')
                $('html, body').animate({
                    scrollTop: $(window).scrollTop() > 225 ? $(this).offset().top - 135 : $(this).offset().top - 145
                }, 700, () => $(this).next().children().css('transition', 'opacity 2s').css('opacity', '1'))
                $.each(data, function (i, e) {
                    if (i === 0) {
                        $('#ReportInfo').append('<h4 class="px-3 py-2">檢舉人與理由:</h4>')
                        return
                    }
                    $('#ReportInfo').append(`<div class="mx-3 my-2 p-2 rounded border border-danger bg-white">
                        <p class="h6">#${i} 檢舉人: ${e.Member}</p>
                        <p class="text-danger pl-2">理由: ${e.Reason}</p>
                    </div>`)
                })
                $('#ContentCard').css('min-height', `${$('#ReportInfo').height() - 70}px`)
            }
        })
    })

    // Search Author
    $('#ArticleManagementSearch').on('click', function (e) {
        swal('查詢作者', '', 'warning', {
            dangerMode: true,
            buttons: {
                cancel: '取消',
                confirm: '查詢作者文章',
                profile: '查詢作者資料'
            }
        }).then(x => {
            if (x === true) {
                $('#ctrlCate').prop('selectedIndex', 0)
                $('#ctrlDate').prop('selectedIndex', 0)
                $('#ctrlReport').prop('selectedIndex', 0)
                $('#searchbox').val('')
                $('#fPageCurrent').val(1)
                $('#fPageSize').val(10)
                $('#fKeyword').val('')
                $('#pageAmount').prop('selectedIndex', 0)
                $('#fAuthorSearch').val($(this).data('id'))
                $('#ctrlBtnC').removeClass('d-none').html(`&times; 取消查詢作者: ${$(this).data('email')}`)
                AjaxArticleList()
            } else if (x === 'profile') {
                theDetail($(this).data('id'))
            }
        })
    })

    // ArticleManagementDelete
    $('#ArticleManagementDelete').on('click', function () {
        let id = $(this).data('id')
        let type = $(this).data('arid')[0]
        let arid = $(this).data('arid').slice(1)
        let warinigText = ''
        if (type === 'A') {
            $.ajax({
                url: '/BackEndArticle/ArticleReplyConfirm',
                type: 'post',
                async: false,
                data: {
                    id: arid
                },
                success: function (data) {
                    if (data === 'True') {
                        warinigText = '<p class="text-warning"><i class="fas fa-exclamation-triangle"></i> 此文章有回覆被檢舉，有必要的話請優先處理回覆。</p>'
                    }
                }
            })
        }
        let html = (`
            <form id="AjaxBoxForm">
                <div class="form-group">
                    <label for="AjaxBoxTextarea">刪除${type === 'A' ? '文章' : '回覆'}並發送系統通知:</label>
                    <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                </div>
            </form>
            ${warinigText}
        `)
        $('#AjaxBoxDemo').data('demo', 'Delete')
        $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
        $('#AjaxBoxBody').html(html)
        $('#AjaxBoxTitle').html(`刪除${type === 'A' ? '文章' : '回覆'}`)
        $('#OK').off('click').on('click', function () {
            if ($('#AjaxBoxTextarea').val().trim() === '') {
                swal('無法進行操作', '請填寫系統通知!', 'error', {
                    button: false
                })
            } else {
                swal(`刪除${type === 'A' ? '文章' : '回覆'}確認`, `是否要刪除${type === 'A' ? '文章' : '回覆'}`, 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal(`刪除${type === 'A' ? '文章' : '回覆'}提示`, '處理中!', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndArticle/ArticleReplyDelete',
                            type: 'post',
                            data: {
                                id: id,
                                message: $('#AjaxBoxTextarea').val().trim(),
                                type: type,
                                arid: arid,
                            },
                            timeout: 20000,
                            success: function (data) {
                                swal(`刪除${type === 'A' ? '文章' : '回覆'}提示`, `刪除${type === 'A' ? '文章' : '回覆'}成功!`, 'success', {
                                    button: false
                                })
                                AjaxArticleList()
                            },
                            error: function (xmlhttprequest, textstatus, message) {
                                swal(`刪除${type === 'A' ? '文章' : '回覆'}提示`, textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                        $(this).prev().click()
                    }
                })
            }
        })
    })

    // ArticleManagementDeleteBan
    $('#ArticleManagementDeleteBan').on('click', function () {
        let tomorrow = `${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, '0')}-${String(new Date().getDate() + 1).padStart(2, '0')}`
        let id = $(this).data('id')
        let type = $(this).data('arid')[0]
        let arid = $(this).data('arid').slice(1)
        let warinigText = ''
        if (type === 'A') {
            $.ajax({
                url: '/BackEndArticle/ArticleReplyConfirm',
                type: 'post',
                async: false,
                data: {
                    id: arid
                },
                success: function (data) {
                    if (data === 'True') {
                        warinigText = '<p class="text-warning"><i class="fas fa-exclamation-triangle"></i> 此文章有回覆被檢舉，有必要的話請優先處理回覆。</p>'
                    }
                }
            })
        }
        let html = (`
            <form id="AjaxBoxForm">
                <div class="form-group">
                    <label for="AjaxBoxTextarea">刪除${type === 'A' ? '文章' : '回覆'}並發送系統通知:</label>
                    <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                </div>
                <div class="form-group">
                    <label for="AjaxBoxTextarea2">停權原因:</label>
                    <textarea id="AjaxBoxTextarea2" class="form-control" rows="3"></textarea>
                </div>
                <div class="form-group">
                    <label for="AjaxBoxDate">停權到期日:</label>
                    <input type="date" id="AjaxBoxDate" class="form-control" value="${tomorrow}" min="${tomorrow}">
                </div>
            </form>
            ${warinigText}
        `)
        $('#AjaxBoxDemo').data('demo', 'DeleteBan')
        $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
        $('#AjaxBoxBody').html(html)
        $('#AjaxBoxTitle').html(`刪除${type === 'A' ? '文章' : '回覆'}`)
        $('#OK').off('click').on('click', function () {
            if ($('#AjaxBoxTextarea').val().trim() === '' || $('#AjaxBoxTextarea2').val().trim() === '') {
                swal('無法進行操作', '請填寫系統通知與停權原因!', 'error', {
                    button: false
                })
            } else {
                swal(`刪除${type === 'A' ? '文章' : '回覆'}確認`, `是否要刪除${type === 'A' ? '文章' : '回覆'}`, 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal(`刪除${type === 'A' ? '文章' : '回覆'}提示`, '處理中!', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndArticle/ArticleReplyDeleteBan',
                            type: 'post',
                            data: {
                                id: id,
                                message: $('#AjaxBoxTextarea').val().trim(),
                                type: type,
                                arid: arid,
                                reason: $('#AjaxBoxTextarea2').val().trim(),
                                endtime: $('#AjaxBoxDate').val()
                            },
                            timeout: 20000,
                            success: function (data) {
                                swal(`刪除${type === 'A' ? '文章' : '回覆'}提示`, `刪除${type === 'A' ? '文章' : '回覆'}成功!`, 'success', {
                                    button: false
                                })
                                AjaxArticleList()
                            },
                            error: function (xmlhttprequest, textstatus, message) {
                                swal(`刪除${type === 'A' ? '文章' : '回覆'}提示`, textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                        $(this).prev().click()
                    }
                })
            }
        })
    })
})