document.addEventListener('DOMContentLoaded', function () {
    // DOMContentLoaded
    $.ajax({
        url: '/BackEndComment/GetSubCateOption',
        type: 'post',
        data: {
            id: $('#ctrlCate').val()
        },
        success: function (data) {
            $('#ctrlSubCate').append('<option value="0">所有子類</option>')
            $.each(data, function (i, e) {
                $('#ctrlSubCate').append(`<option value="${e.SubCategoryId}">${e.SubCategoryName}</option>`)
            })
            $('#ctrlSubCate').prop('selectedIndex', 0)
            AjaxCommentList()
        }
    })

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
    }).on('mouseenter', '#CommentManagementUrl', function () {
        $(this).addClass('text-danger')
    }).on('mouseleave', '#CommentManagementUrl', function () {
        $(this).removeClass('text-danger')
    })

    // PageCurrent Change
    $('#pageTop, #pageBottom').on('click', '.customGotoPage', function () {
        $('#fPageCurrent').val(parseInt($(this).data('id')))
        AjaxCommentList()
    }).on('click', '.customGotoNewPage', function () {
        swal({
            title: '請輸入頁碼',
            content: 'input'
        }).then(page => {
            if (page !== null && /^(?=.*[1-9])[0-9]+$/.test(page)) {
                $('#fPageCurrent').val(Math.min(parseInt(page), maxpage))
                AjaxCommentList()
            }
        })
    })

    // PageSize Change
    $('#pageAmount').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val($(this).val())
        AjaxCommentList()
    })

    // Keyword Change
    $('#searchbox').on('input', keyInput).on('compositionstart', function () {
        $(this).off('input')
    }).on('compositionend', function () {
        keyInput()
        $(this).on('input', keyInput)
    })

    // ctrlCate change
    $('#ctrlCate').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#ctrlSubCate').empty()
        $.ajax({
            url: '/BackEndComment/GetSubCateOption',
            type: 'post',
            data: {
                id: $(this).val()
            },
            success: function (data) {
                $('#ctrlSubCate').append('<option value="0">所有子類</option>')
                $.each(data, function (i, e) {
                    $('#ctrlSubCate').append(`<option value="${e.SubCategoryId}">${e.SubCategoryName}</option>`)
                })
                $('#ctrlSubCate').prop('selectedIndex', 0)
                AjaxCommentList()
            }
        })
    })

    // ctrlSubCate / ctrlDate / ctrlReport / ctrlShowBan Change
    $('#ctrlSubCate, #ctrlDate, #ctrlReport, #ctrlShowBan').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        AjaxCommentList()
    })

    // ctrlBtn - ctrlBtnC
    $('#ctrlBtnC').on('click', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#fAuthorSearch').val(0)
        $(this).addClass('d-none').html('')
        AjaxCommentList()
    })

    // ListBody
    $('#ListBody').on('mouseenter', 'tr:not(".CommentDetail")', function () {
        $(this).addClass('bg-warning')
    }).on('mouseleave', 'tr:not(".CommentDetail")', function () {
        $(this).removeClass('bg-warning')
    }).on('click', 'tr:not(".CommentDetail")', function () {
        $('.CommentDetail').remove()
        if ($(this).hasClass('currentDetail')) {
            $(this).css('background-color', 'white').removeClass('currentDetail')
            $('#CommentManagement').addClass('d-none')
            return
        }
        $(this).css('background-color', '#fffdaf').addClass('currentDetail')
            .siblings().css('background-color', 'white').removeClass('currentDetail')
        $.ajax({
            url: '/BackEndComment/CommentDetail',
            type: 'post',
            data: {
                id: $(this).data('id')
            },
            success: (data) => {
                $('#CommentManagement').removeClass('d-none')
                if (data[0].IsBaned) {
                    $('#CommentManagementHide').addClass('d-none')
                    $('#CommentManagementHideBan').addClass('d-none')
                    $('#CommentManagementShow').removeClass('d-none')
                } else {
                    $('#CommentManagementHide').removeClass('d-none')
                    $('#CommentManagementHideBan').removeClass('d-none')
                    $('#CommentManagementShow').addClass('d-none')
                }
                $('#CommentManagementUrl').text(data[0].ActivityTitle)
                    .attr('href', `/Activity/ActivityDetail?activityId=${data[0].ActivityId}`)
                $('#CommentManagementHide').data('id', `${data[0].MemberId}`).data('cid', `${data[0].CommentId}`)
                $('#CommentManagementHideBan').data('id', `${data[0].MemberId}`).data('cid', `${data[0].CommentId}`)
                $('#CommentManagementShow').data('id', `${data[0].MemberId}`).data('cid', `${data[0].CommentId}`)
                $('#CommentManagementSearch').data('id', `${data[0].MemberId}`).data('email', `${data[0].MemberEmail}`)
                $('#CommentManagementFlag').html(`<i class="fas fa-flag mr-1"></i> 檢舉數: ${$(this).children().last().text()}`)
                $(this).after(`<tr class="CommentDetail" style="background-color: lightyellow">
                    <td colspan="2" class="p-0" style="opacity: 0;">
                        <div class="preventLink"></div>
                    </td>
                    <td colspan="4" class="p-0" style="opacity: 0;">
                        <div id="ReportInfo" style="display: grid; grid-template-columns: 1fr 1fr 1fr;"></div>
                    </td>
                </tr>`)
                $('.preventLink').html(`
                    <h5 class="px-3 py-2">使用者 
                        <a id="CommentAuthorSearch" href="javascript: $('#CommentManagementSearch').click()" class="text-info text-decoration-none">${data[0].MemberEmail}</a>
                        的評論內容:
                    </h5>
                    <div id="ContentCard" class="card m-auto" style="width: 90%;">
                        <h6 class="card-header" style="background-color: #fffdaf;">${data[0].ActivityTitle}</h6>
                        <div class="card-body">${data[0].Content}</div>
                    </div>
                `)
                $('html, body').animate({
                    scrollTop: $(window).scrollTop() > 225 ? $(this).offset().top - 135 : $(this).offset().top - 145
                }, 700, () => $(this).next().children().css('transition', 'opacity 2s').css('opacity', '1'))
                $.each(data, function (i, e){
                    if (i === 0) {
                        $('#ReportInfo').before('<h5 class="px-3 py-2">檢舉人與理由:</h5>')
                        return
                    }
                    $('#ReportInfo').append(`<div class="mx-3 my-2 p-2 rounded border border-danger bg-white">
                        <p class="h6">#${i} 檢舉人: ${e.Member}</p>
                        <p class="text-danger pl-2">理由: ${e.Reason}</p>
                    </div>`)
                })
            }
        })
    })

    // Search Author
    $('#CommentManagementSearch').on('click', function (e) {
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
                $.ajax({
                    url: '/BackEndComment/GetSubCateOption',
                    type: 'post',
                    data: {
                        id: $('#ctrlCate').val()
                    },
                    success: function (data) {
                        $('#ctrlSubCate').append('<option value="0">所有子類</option>')
                        $.each(data, function (i, e) {
                            $('#ctrlSubCate').append(`<option value="${e.SubCategoryId}">${e.SubCategoryName}</option>`)
                        })
                        $('#ctrlSubCate').prop('selectedIndex', 0)
                    }
                })
                $('#ctrlDate').prop('selectedIndex', 0)
                $('#ctrlReport').prop('selectedIndex', 0)
                $('#ctrlShowBan').prop('selectedIndex', 1)
                $('#searchbox').val('')
                $('#fPageCurrent').val(1)
                $('#fPageSize').val(10)
                $('#fKeyword').val('')
                $('#pageAmount').prop('selectedIndex', 0)
                $('#fAuthorSearch').val($(this).data('id'))
                $('#ctrlBtnC').removeClass('d-none').html(`&times; 取消查詢作者: ${$(this).data('email')}`)
                AjaxCommentList()
            } else if (x === 'profile') {
                theDetail($(this).data('id'))
            }
        })
    })

    // CommentManagementHide
    $('#CommentManagementHide').on('click', function () {
        let id = $(this).data('id')
        let cid = $(this).data('cid')
        let html = (`
            <form id="AjaxBoxForm">
                <div class="form-group">
                    <label for="AjaxBoxTextarea">隱藏評論並發送系統通知:</label>
                    <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                </div>
            </form>
        `)
        $('#AjaxBoxDemo').data('demo', 'Hide')
        $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
        $('#AjaxBoxBody').html(html)
        $('#AjaxBoxTitle').html('隱藏評論')
        $('#OK').off('click').on('click', function () {
            if ($('#AjaxBoxTextarea').val().trim() === '') {
                swal('無法進行操作', '請填寫系統通知!', 'error', {
                    button: false
                })
            } else {
                swal('隱藏評論確認', '是否要隱藏評論', 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal('隱藏評論提示', '處理中!', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndComment/CommentHide',
                            type: 'post',
                            data: {
                                id: id,
                                message: $('#AjaxBoxTextarea').val().trim(),
                                cid: cid,
                            },
                            timeout: 20000,
                            success: function (data) {
                                swal('隱藏評論提示', '隱藏評論成功!', 'success', {
                                    button: false
                                })
                                AjaxCommentList()
                            },
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('隱藏評論提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
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

    // CommentManagementHideBan
    $('#CommentManagementHideBan').on('click', function () {
        let tomorrow = `${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, '0')}-${String(new Date().getDate() + 1).padStart(2, '0')}`
        let id = $(this).data('id')
        let cid = $(this).data('cid')
        let html = (`
            <form id="AjaxBoxForm">
                <div class="form-group">
                    <label for="AjaxBoxTextarea">隱藏評論並發送系統通知:</label>
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
        `)
        $('#AjaxBoxDemo').data('demo', 'HideBan')
        $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
        $('#AjaxBoxBody').html(html)
        $('#AjaxBoxTitle').html('隱藏評論')
        $('#OK').off('click').on('click', function () {
            if ($('#AjaxBoxTextarea').val().trim() === '' || $('#AjaxBoxTextarea2').val().trim() === '') {
                swal('無法進行操作', '請填寫系統通知與停權原因!', 'error', {
                    button: false
                })
            } else {
                swal('隱藏評論確認', '是否要隱藏評論', 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal('隱藏評論提示', '處理中!', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndComment/CommentHideBan',
                            type: 'post',
                            data: {
                                id: id,
                                message: $('#AjaxBoxTextarea').val().trim(),
                                cid: cid,
                                reason: $('#AjaxBoxTextarea2').val().trim(),
                                endtime: $('#AjaxBoxDate').val()
                            },
                            timeout: 20000,
                            success: function (data) {
                                swal('隱藏評論提示', '隱藏評論成功!', 'success', {
                                    button: false
                                })
                                AjaxCommentList()
                            },
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('隱藏評論提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
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

    // CommentManagementShow
    $('#CommentManagementShow').on('click', function () {
        let id = $(this).data('id')
        let cid = $(this).data('cid')
        swal('解除隱藏評論確認', '是否要解除隱藏評論', 'warning', {
            buttons: {
                cancel: '取消',
                confirm: '確定'
            }
        }).then(x => {
            if (x) {
                swal('解除隱藏評論提示', '處理中!', 'info', {
                    button: false,
                    closeOnClickOutside: false,
                    closeOnEsc: false,
                })
                $.ajax({
                    url: '/BackEndComment/CommentShow',
                    type: 'post',
                    data: {
                        id: id,
                        cid: cid
                    },
                    timeout: 20000,
                    success: function (data) {
                        swal('解除隱藏評論提示', '解除隱藏評論成功!', 'success', {
                            button: false
                        })
                        AjaxCommentList()
                    },
                    error: function (xmlhttprequest, textstatus, message) {
                        swal('解除隱藏評論提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                            button: false
                        })
                    }
                })
            }
        })
    })
})