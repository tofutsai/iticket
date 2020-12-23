document.addEventListener('DOMContentLoaded', function () {
    // DOMContentLoaded
    AjaxMemberList()
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
    })

    // PageCurrent Change
    $('#pageTop, #pageBottom').on('click', '.customGotoPage', function () {
        $('#fPageCurrent').val(parseInt($(this).data('id')))
        AjaxMemberList()
    }).on('click', '.customGotoNewPage', function () {
        swal({
            title: '請輸入頁碼',
            content: 'input'
        }).then(page => {
            if (page !== null && /^(?=.*[1-9])[0-9]+$/.test(page)) {
                $('#fPageCurrent').val(Math.min(parseInt(page), maxpage))
                AjaxMemberList()
            }
        })
    })

    // PageSize Change
    $('#pageAmount').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val($(this).val())
        AjaxMemberList()
    })

    // Keyword Change
    $('#searchbox').on('input', keyInput).on('compositionstart', function () {
        $(this).off('input')
    }).on('compositionend', function () {
        keyInput()
        $(this).on('input', keyInput)
    })

    // ctrlBtn - ctrlBtn0
    $('#ctrlBtn0').on('click', function () {
        if (!members.length) {
            let rollmembers
            switch (parseInt($('#fRoleId').val())) {
                case 0:
                    rollmembers = '停權會員'
                    break
                case 1:
                    rollmembers = '未驗證會員'
                    break
                case 2:
                    rollmembers = '普通會員'
                    break
                case 3:
                    rollmembers = '商家'
                    break
                default:
                    rollmembers = ''
                    break
            }
            swal('沒有選取任何對象', '是否發送給所有會員?', 'warning', {
                dangerMode: true,
                buttons: {
                    cancel: '取消',
                    confirm: '確定',
                    roll: {
                        text: `發送給所有${rollmembers}`,
                        visible: rollmembers
                    }
                }
            }).then(x => {
                if (x === null) {
                    return
                } else if (x === true) {
                    let html = (`
                        <form id="AjaxBoxForm">
                            <div class="form-group">
                                <label for="AjaxBoxTextarea">系統通知:</label>
                                <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                            </div>
                        </form>
                        <p>發送的對象:</p>
                        <p class="text-success" style="cursor: pointer"><i class="far fa-bell"></i> To: 所有會員</p>
                    `)
                    $('#AjaxBoxDemo').data('demo', 'Send')
                    $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
                    $('#AjaxBoxBody').html(html)
                    $('#AjaxBoxTitle').html(`<i class="far fa-bell"></i> 發送系統通知 To: 所有會員`)
                    $('#OK').off('click').on('click', function () {
                        if ($('#AjaxBoxTextarea').val().trim() === '') {
                            swal('無法發送系統通知', '請填寫系統通知!', 'error', {
                                button: false
                            })
                        } else {
                            swal('發送系統通知確認', '確定要送出系統通知嗎?', 'warning', {
                                buttons: {
                                    cancel: '取消',
                                    confirm: '確定'
                                }
                            }).then(y => {
                                if (y) {
                                    swal('發送系統通知提示', '系統通知發送中!', 'info', {
                                        button: false,
                                        closeOnClickOutside: false,
                                        closeOnEsc: false,
                                    })
                                    members.length = 0
                                    emails.length = 0
                                    AjaxMemberList()
                                    SendMessageTask('All', 'All', $('#AjaxBoxTextarea').val().trim())
                                    $(this).prev().click()
                                }
                            })
                        }
                    })
                } else {
                    let html = (`
                        <form id="AjaxBoxForm">
                            <div class="form-group">
                                <label for="AjaxBoxTextarea">系統通知:</label>
                                <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                            </div>
                        </form>
                        <p>發送的對象:</p>
                        <p class="text-success" style="cursor: pointer"><i class="far fa-bell"></i> To: 所有${rollmembers}</p>
                    `)
                    $('#AjaxBoxDemo').data('demo', 'Send')
                    $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
                    $('#AjaxBoxBody').html(html)
                    $('#AjaxBoxTitle').html(`<i class="far fa-bell"></i> 發送系統通知 To: 所有${rollmembers}`)
                    $('#OK').off('click').on('click', function () {
                        if ($('#AjaxBoxTextarea').val().trim() === '') {
                            swal('無法發送系統通知', '請填寫系統通知!', 'error', {
                                button: false
                            })
                        } else {
                            swal('發送系統通知確認', '確定要送出系統通知嗎?', 'warning', {
                                buttons: {
                                    cancel: '取消',
                                    confirm: '確定'
                                }
                            }).then(y => {
                                if (y) {
                                    swal('發送系統通知提示', '系統通知發送中!', 'info', {
                                        button: false,
                                        closeOnClickOutside: false,
                                        closeOnEsc: false,
                                    })
                                    members.length = 0
                                    emails.length = 0
                                    AjaxMemberList()
                                    SendMessageTask(rollmembers, rollmembers, $('#AjaxBoxTextarea').val().trim())
                                    $(this).prev().click()
                                }
                            })
                        }
                    })
                }
            })
        } else {
            let info = ''
            for (let e of emails) {
                info += `<p class="text-success" style="cursor: pointer"><i class="far fa-bell"></i> ${e}</p>`
            }
            let html = (`
                <form id="AjaxBoxForm">
                    <div class="form-group">
                        <label for="AjaxBoxTextarea">系統通知:</label>
                        <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                    </div>
                </form>
                <p>發送的對象:</p>
                ${info}
            `)
            $('#AjaxBoxDemo').data('demo', 'Send')
            $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
            $('#AjaxBoxBody').html(html)
            $('#AjaxBoxTitle').html(`<i class="far fa-bell"></i> 發送系統通知 To: 選取的 ${members.length} 個會員`)
            $('#OK').off('click').on('click', function () {
                if ($('#AjaxBoxTextarea').val().trim() === '') {
                    swal('無法發送系統通知', '請填寫系統通知!', 'error', {
                        button: false
                    })
                } else {
                    swal('發送系統通知確認', '確定要送出系統通知嗎?', 'warning', {
                        buttons: {
                            cancel: '取消',
                            confirm: '確定'
                        }
                    }).then(y => {
                        if (y) {
                            swal('發送系統通知提示', '系統通知發送中!', 'info', {
                                button: false,
                                closeOnClickOutside: false,
                                closeOnEsc: false,
                            })
                            let memberlist = members.join()
                            members.length = 0
                            emails.length = 0
                            AjaxMemberList()
                            SendMessageTask('指定會員', memberlist, $('#AjaxBoxTextarea').val().trim())
                            $(this).prev().click()
                        }
                    })
                }
            })
        }
    })

    // ctrlBtn - ctrlBtnA
    $('#ctrlBtnA').on('mouseenter', function () {
        $(this).addClass('btn-outline-warning')
    }).on('mouseleave', function () {
        if (parseInt($('#fRoleId').val()) !== 9) {
            $(this).removeClass('btn-outline-warning')
        }
    }).on('click', function () {
        if (parseInt($('#fRoleId').val()) === 9) {
            return
        }
        $(this).addClass('btn-outline-warning').siblings().removeClass('btn-outline-warning').removeClass('btn-outline-danger')
        $('#ctrlBtn6').addClass('d-none')
        $('#ctrlBtn7').removeClass('d-none')
        $('#fRoleId').val(9)
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#fSortRule').val("0")
        $('#pageAmount').prop('selectedIndex', 0)
        MemberRoleInfoFont($('#fSortRule').val())
        members.length = 0
        emails.length = 0
        AjaxMemberList()
    })

    // ctrlBtn - ctrlBtn1, ctrlBtn2, ctrlBtn3, ctrlBtn4
    $('#ctrlBtn1, #ctrlBtn2, #ctrlBtn3, #ctrlBtn4').on('mouseenter', function () {
        let i = 4 - parseInt(this.id.slice(-1))
        $(this).addClass(i ? 'btn-outline-warning' : 'btn-outline-danger')
    }).on('mouseleave', function () {
        let i = 4 - parseInt(this.id.slice(-1))
        if (parseInt($('#fRoleId').val()) !== i) {
            $(this).removeClass(i ? 'btn-outline-warning' : 'btn-outline-danger')
        }
    }).on('click', function () {
        let i = 4 - parseInt(this.id.slice(-1))
        if (parseInt($('#fRoleId').val()) === i) {
            return
        }
        $(this).addClass(i ? 'btn-outline-warning' : 'btn-outline-danger')
            .siblings().removeClass('btn-outline-warning').removeClass('btn-outline-danger')
        $('#ctrlBtn6').addClass('d-none')
        $('#ctrlBtn7').addClass('d-none')
        if (i == 3) {
            $('#fNonVerify').val(0)
            $('#ctrlBtn6').html('&#x2610; 未審核商家').removeClass('d-none')
        }
        $('#fRoleId').val(i)
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#fSortRule').val("0")
        $('#pageAmount').prop('selectedIndex', 0)
        MemberRoleInfoFont($('#fSortRule').val())
        members.length = 0
        emails.length = 0
        AjaxMemberList()
    })

    // ctrlBtn - ctrlBtn5
    $('#ctrlBtn5').on('click', function () {
        $('#fPageCurrent').val(1)
        $('#fSortRule').val('0')
        MemberRoleInfoFont($('#fSortRule').val())
        AjaxMemberList()
    })

    // ctrlBtn - ctrlBtn6
    $('#ctrlBtn6').on('click', function () {
        if (parseInt($('#fNonVerify').val()) == 0) {
            $('#fNonVerify').val(1)
            $('#ctrlBtn6').html('&#x2612; 未審核商家')
        } else {
            $('#fNonVerify').val(0)
            $('#ctrlBtn6').html('&#x2610; 未審核商家')
        }
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#fSortRule').val("0")
        $('#pageAmount').prop('selectedIndex', 0)
        MemberRoleInfoFont($('#fSortRule').val())
        AjaxMemberList()
    })

    // ctrlBtn - ctrlBtn7
    $('#ctrlBtn7').on('click', function () {
        $('#ctrlBtn1').click()
    })

    // ListHead - ListHead0
    $('#ListHead0').on('click', function () {
        const idhere = []
        const emailhere = []
        $('#ListBody tr td:nth-of-type(1)').each(function () {
            idhere.push($(this).data('id'))
            emailhere.push($(this).data('email'))
        })
        let flag = false
        for (let id of idhere) {
            if (members.indexOf(id) == -1) {
                flag = true
                break
            }
        }
        if (flag) {
            $('#ListBody tr td:nth-of-type(1)').each(function (i) {
                let index = members.indexOf(idhere[i])
                if (index == -1) {
                    members.push(idhere[i])
                    emails.push(emailhere[i])
                    $(this).html('<i class="far fa-check-square"></i>').closest('tr').css('background-color', 'lightyellow')
                }
            })
        } else {
            $('#ListBody tr td:nth-of-type(1)').each(function (i) {
                let index = members.indexOf(idhere[i])
                if (index >= 0) {
                    members.splice(index, 1)
                    emails.splice(index, 1)
                    $(this).html('<i class="far fa-square"></i>').closest('tr').css('background-color', 'white')
                }
            })
        }
    })

    // ListHead - ListHead1, ListHead2, ListHead3, ListHead4
    $('#ListHead1, #ListHead2, #ListHead3, #ListHead4').on('click', function () {
        let i = this.id.slice(-1)
        $('#fPageCurrent').val(1)
        $('#fSortRule').val($('#fSortRule').val() === `${i}a` ? `${i}d` : `${i}a`)
        MemberRoleInfoFont($('#fSortRule').val())
        AjaxMemberList()
    })

    // ListBody
    $('#ListBody').on('click', '.customSelectUser', function () {
        let id = $(this).data('id')
        let email = $(this).data('email')
        let index = members.indexOf(id)
        if (index == -1) {
            members.push(id)
            emails.push(email)
            $(this).html('<i class="far fa-check-square"></i>').closest('tr').css('background-color', 'lightyellow')
        } else {
            members.splice(index, 1)
            emails.splice(index, 1)
            $(this).html('<i class="far fa-square"></i>').closest('tr').css('background-color', 'white')
        }
    }).on('click', '.customBan', function () {
        let tomorrow = `${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, '0')}-${String(new Date().getDate() + 1).padStart(2, '0')}`
        let id = $(this).data('id')
        let email = $(this).data('email')
        let html = (`
            <form id="AjaxBoxForm">
                <div class="form-group">
                    <label for="AjaxBoxTextarea">停權原因:</label>
                    <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                </div>
                <div class="form-group">
                    <label for="AjaxBoxDate">停權到期日:</label>
                    <input type="date" id="AjaxBoxDate" class="form-control" value="${tomorrow}" min="${tomorrow}">
                </div>
            </form>
        `)
        $('#AjaxBoxDemo').data('demo', 'Ban')
        $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
        $('#AjaxBoxBody').html(html)
        $('#AjaxBoxTitle').html(`停權 ${email}`)
        $('#OK').off('click').on('click', function () {
            if ($('#AjaxBoxTextarea').val().trim() === '') {
                swal('無法進行停權操作', '請填寫停權原因!', 'error', {
                    button: false
                })
            } else {
                swal(`停權會員確認`, `確定要停權 ${email} ?`, 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal('停權會員提示', '處理中!', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndMember/BanMember',
                            type: 'post',
                            data: {
                                id: id,
                                reason: $('#AjaxBoxTextarea').val().trim(),
                                endtime: $('#AjaxBoxDate').val()
                            },
                            timeout: 20000,
                            success: function (data) {
                                swal('停權會員提示', '停權成功!', 'success', {
                                    button: false
                                })
                                AjaxMemberList()
                            },
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('停權會員提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                        $(this).prev().click()
                    }
                })
            }
        })
    }).on('click', '.customUnBan', function () {
        let id = $(this).data('id')
        let email = $(this).data('email')
        swal(`解除停權會員提示`, `是否要恢復 ${email} 的權限?`, 'warning', {
            buttons: {
                cancel: '取消',
                confirm: '確定'
            }
        }).then(x => {
            if (x) {
                swal('解除停權會員提示', '處理中!', 'info', {
                    button: false,
                    closeOnClickOutside: false,
                    closeOnEsc: false,
                })
                $.ajax({
                    url: '/BackEndMember/UnBanMember',
                    type: 'post',
                    data: {
                        id: id
                    },
                    timeout: 20000,
                    success: function (data) {
                        swal('解除停權會員提示', '解除成功!', 'success', {
                            button: false
                        })
                        AjaxMemberList()
                    },
                    error: function (xmlhttprequest, textstatus, message) {
                        swal('解除停權會員提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                            button: false
                        })
                    }
                })
            }
        })
    }).on('click', '.customFile', function () {
        let id = $(this).data('id')
        let email = $(this).data('email')
        $.ajax({
            url: '/BackEndMember/DataDownloadCheck',
            type: 'post',
            data: {
                id: id
            },
            success: function (data) {
                if (data === 'Success') {
                    swal('商家資料', `是否要下載 ${email} 商家的資料?`, 'warning', {
                        buttons: {
                            cancel: '取消',
                            confirm: '下載'
                        }
                    }).then(x => {
                        if (x) {
                            window.open(`/BackEndMember/DataDownload/${id}`)
                            swal('商家資料', '下載完成', 'success', {
                                button: false
                            })
                        }
                    })
                } else {
                    swal('商家資料', '商家並未上傳資料', 'error', {
                        button: false
                    })
                }
            }
        })
    }).on('click', '.customVerify', function () {
        let id = $(this).data('id')
        let email = $(this).data('email')
        swal('審核商家提示', `是否要通過 ${email} 商家的審核?`, 'warning', {
            dangerMode: true,
            buttons: {
                cancel: '取消',
                confirm: '駁回',
                pass: '通過'
            }
        }).then(x => {
            if (x === 'pass') {
                swal('審核商家提示', '處理中!', 'info', {
                    button: false,
                    closeOnClickOutside: false,
                    closeOnEsc: false,
                })
                $.ajax({
                    url: '/BackEndMember/MerchantVerification',
                    type: 'post',
                    data: {
                        id: id,
                        fPass: true
                    },
                    timeout: 20000,
                    success: function (data) {
                        swal('審核商家提示', '審核通過!', 'success', {
                            button: false
                        })
                        AjaxMemberList()
                    },
                    error: function (xmlhttprequest, textstatus, message) {
                        swal('審核商家提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                            button: false
                        })
                    }
                })
            } else if (x === true) {
                let html = (`
                    <form id="AjaxBoxForm">
                        <div class="form-group">
                            <label for="AjaxBoxTextarea">駁回原因:</label>
                            <textarea id="AjaxBoxTextarea" class="form-control" rows="3"></textarea>
                        </div>
                    </form>
                `)
                $('#AjaxBoxDemo').data('demo', 'fPassFalse')
                $('#AjaxBox').modal({ backdrop: 'static', keyboard: false, show: true })
                $('#AjaxBoxBody').html(html)
                $('#AjaxBoxTitle').html(`審核駁回`)
                $('#OK').off('click').on('click', function () {
                    if ($('#AjaxBoxTextarea').val().trim() === '') {
                        swal('無法進行審核操作', '請填寫駁回原因!', 'error', {
                            button: false
                        })
                    } else {
                        swal('審核商家提示', '處理中!', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndMember/MerchantVerification',
                            type: 'post',
                            data: {
                                id: id,
                                fPass: false,
                                reason: $('#AjaxBoxTextarea').val().trim()
                            },
                            timeout: 20000,
                            success: function (data) {
                                swal('審核商家提示', '審核駁回!', 'success', {
                                    button: false
                                })
                                AjaxMemberList()
                                $('#OK').prev().click()
                            },
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('審核商家提示', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                                $('#OK').prev().click()
                            }
                        })
                    }
                })
            }
        })
    })
})