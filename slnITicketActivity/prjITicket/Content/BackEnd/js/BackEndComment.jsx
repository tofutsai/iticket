function AjaxCommentList() {
    $('#isHandle').removeClass('d-none')
    $.ajax({
        url: '/BackEndComment/CommentList',
        type: 'post',
        data: $('#ctrlForm').serialize(),
        success: function (data) {
            if (data[0].ChangePage != 0) {
                $('#fPageCurrent').val(data[0].ChangePage)
            }
            $('#ctrlHint').text($('#fKeyword').val() == '' ? ''
                : $('#fKeyword').val().startsWith('author:') ? `查詢作者: ${$('#fKeyword').val().split(':')[1].trim()}`
                : `關鍵字: ${$('#fKeyword').val()}`)
            ReactDOM.render(<CommentList data={data.slice(1)} />, document.querySelector('#listBody'))
            let maxpage = data[0].MaxPage
            let current = parseInt($('#fPageCurrent').val())
            ReactDOM.render(<Pagination page={current} max={maxpage} />, document.querySelector('#pageTop'))
            ReactDOM.render(<Pagination page={current} max={maxpage} />, document.querySelector('#pageBottom'))
            let begin = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + 1
            let ending = ($('#fPageCurrent').val() - 1) * $('#fPageSize').val() + data.length - 1
            $('#pageMessage').text(begin <= ending ? `顯示第 ${begin} 筆到第 ${ending} 筆資料` : `沒有符合的資料`)
            $('#isHandle').addClass('d-none')
        }
    })
}

class CommentDetail extends React.Component {
    openActivityPage = (ActivityName) => {
        let target = window.open(`/Activity/ActivityList#action=keyword&keyword=${ActivityName}`)
        target.addEventListener('DOMContentLoaded', function () {
            let script = target.document.createElement('script')
            script.innerHTML = '$(window).scrollTop(567)'
            target.document.body.appendChild(script)
        })
    }
    render() {
        let data = this.props.data
        return (
            <div className="row">
                <div className="col pl-3 pr-1 py-3">
                    <div className="card text-body mb-1">
                        <div className="card-header">
                            <h5>{data[0].MemberEmail.split('@')[0]} 的評論內容
                                {
                                    data[0].IsBaned &&
                                    <span className="badge badge-danger ml-2">已隱藏</span>
                                }
                            </h5>
                        </div>
                        <div className="card-body">
                            <p className="text-warning">
                                {data[0].CommentScore > 0 ? '★' : '☆'}
                                {data[0].CommentScore > 1 ? '★' : '☆'}
                                {data[0].CommentScore > 2 ? '★' : '☆'}
                                {data[0].CommentScore > 3 ? '★' : '☆'}
                                {data[0].CommentScore > 4 ? '★' : '☆'}
                            </p>
                            <p>{data[0].CommentContent}</p>
                        </div>
                    </div>
                    <div className="card text-body mt-1" style={{ minHeight: `${70 * (data.length - 2.5)}px` }}>
                        <div className="card-header">
                            <h5>活動: &nbsp;
                                <a href="javascript:"
                                    onClick={() => this.openActivityPage(data[0].ActivityName)}>
                                    {data[0].ActivityName}
                                </a>
                            </h5>
                        </div>
                        <div className="card-body">
                            <p>{data[0].ActivityDescription}</p>
                        </div>
                    </div>
                </div>
                <div className="col pl-1 pr-3 py-3">
                    <div className="card text-body" style={{ height: '100%' }}>
                        <div className="card-header">
                            <h5>檢舉人與理由</h5>
                        </div>
                        <div className="card-body">
                            {data.slice(1).map(e =>
                                <div>
                                    <h6>#{data.indexOf(e)} 檢舉人: {e.ReportEmail.split('@')[0]}</h6>
                                    <p className="text-danger">&nbsp;&nbsp;{e.ReportReason}</p>
                                </div>
                            )}
                            {
                                data.length == 1 &&
                                <span className="text-secondary">評論沒有被檢舉</span>
                            }
                        </div>
                    </div>
                </div>
            </div>
        )
    }
}

class CommentList extends React.Component {
    handleMouseEnter = x => {
        x.style.backgroundColor = 'lightyellow'
        x.querySelector('td:first-child').innerHTML = '<i class="far fa-dot-circle"></i>'
    }
    handleMouseLeave = x => {
        x.style.backgroundColor = 'white'
        x.querySelector('td:first-child').innerHTML = '<i class="far fa-circle"></i>'
    }
    handleClick = id => {
        $('#AjaxBoxTag3-tab').addClass('d-none')
        $('#AjaxBoxTag5-tab').addClass('d-none')
        $('#SearchAuthor').removeClass('d-none')
        $('#BanComment').removeClass('d-none')
        $('#DismissComment').removeClass('d-none')
        $('#Confirm').addClass('d-none')
        $.ajax({
            url: '/BackEndComment/CommentDetail',
            type: 'post',
            data: {
                id: id
            },
            success: function (data) {
                ReactDOM.render(<CommentDetail data={data} />, document.querySelector('#AjaxBoxTag1'))
                ReactDOM.render(<MemberDetail tag={2} data={data[0]} />, document.querySelector('#AjaxBoxTag2'))
                ReactDOM.render(<MemberDetail tag={3} data={data[0]} />, document.querySelector('#AjaxBoxTag3'))
                ReactDOM.render(<MemberDetail tag={0} data={data[0]} />, document.querySelector('#AjaxBoxTag4'))
                if (data[0].IsBaned) {
                    $('#BanComment').addClass('d-none')
                } else {
                    $('#DismissComment').addClass('d-none')
                }
                if (data[0].SellerID !== null) {
                    $('#AjaxBoxTag3-tab').removeClass('d-none')
                }
                $('#AjaxBoxTag1-tab').tab('show')
                $('#SearchAuthor').data('email', data[0].MemberEmail)
                $('#BanComment').data('mid', data[0].MemberID).data('xid', data[0].CommentID)
                $('#DismissComment').data('mid', data[0].MemberID).data('xid', data[0].CommentID)
                $('#AjaxBox').modal({ show: true })
            }
        })
    }
    render() {
        let fKeyword = document.querySelector('#fKeyword').value
        let chngstar = '/images/Activity/CommentStar/chngstar.gif'
        let star = '/images/Activity/CommentStar/star.gif'
        return (
            <React.Fragment>
                {this.props.data.map(e =>
                    <tr data-id={e.CommentID} className={`${e.IsBaned ? 'text-danger' : ''}`} style={{ cursor: 'pointer' }}
                        onMouseEnter={(e) => this.handleMouseEnter(e.currentTarget)}
                        onMouseLeave={(e) => this.handleMouseLeave(e.currentTarget)}
                        onClick={(e) => this.handleClick(e.currentTarget.dataset.id)}
                    >
                        <td className="pt-2 pb-1"><i className="far fa-circle"></i></td>
                        <td className="pt-2 pb-1">
                            <span dangerouslySetInnerHTML={{ __html: keyHighlight(e.Author.split('@', 2)[0], fKeyword) }}></span>
                            <br />
                            <sup className="text-secondary">&nbsp;&nbsp;&nbsp;@{e.Author.split('@', 2)[1]}</sup>
                        </td>
                        <td className="pt-2 pb-1" dangerouslySetInnerHTML={{ __html: keyHighlight(e.Activity, fKeyword) }}></td>
                        <td className="pt-2 pb-1">
                            <img src={e.Score > 0 ? chngstar : star} style={{ width: '24px', margin: '1px' }} />
                            <img src={e.Score > 1 ? chngstar : star} style={{ width: '24px', margin: '1px' }} />
                            <img src={e.Score > 2 ? chngstar : star} style={{ width: '24px', margin: '1px' }} />
                            <img src={e.Score > 3 ? chngstar : star} style={{ width: '24px', margin: '1px' }} />
                            <img src={e.Score > 4 ? chngstar : star} style={{ width: '24px', margin: '1px' }} />
                        </td>
                        <td className="pt-2 pb-1">{e.Date.split(' ')[0]}&nbsp;&nbsp;&nbsp;{e.Date.split(' ')[1]}</td>
                        <td className="pt-2 pb-1">{e.IsBaned ? '已隱藏' : '未隱藏'}</td>
                        <td className="pt-2 pb-1">{e.ReportCount}</td>
                    </tr>
                )}
            </React.Fragment>
        )
    }
}

class GetSubCateOption extends React.Component {
    render() {
        return (
            <React.Fragment>
                <option value="0">所有子類</option>
                {this.props.data.map(e => <option value={e.SubCategoryId}>{e.SubCategoryName}</option>)}
            </React.Fragment>
        )
    }
}

$(function () {
    // DOMContentLoaded
    $.ajax({
        url: '/BackEndComment/GetSubCateOption',
        type: 'post',
        data: {
            id: $('#ctrlCate').val()
        },
        success: function (data) {
            ReactDOM.render(<GetSubCateOption data={data} />, document.querySelector('#ctrlSubCate'))
            $('#ctrlSubCate').prop('selectedIndex', 0)
            AjaxCommentList()
        }
    })

    // Basic - PageSize Change
    $('#pageAmount').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val($(this).val())
        AjaxCommentList()
    })

    // Basic - Keyword Change
    $('#searchbox').on('input', keyInput).on('compositionstart', function () {
        $(this).off('input')
    }).on('compositionend', function () {
        keyInput()
        $(this).on('input', keyInput)
    })

    // Basic - Sort Change
    $(':radio[name="ctrlSort"]').on('change', function () {
        $('#fSort').val(parseInt($(this).val()))
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        AjaxCommentList()
    })

    // Nav - ctrlCate change
    $('#ctrlCate').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $.ajax({
            url: '/BackEndComment/GetSubCateOption',
            type: 'post',
            data: {
                id: $('#ctrlCate').val()
            },
            success: function (data) {
                ReactDOM.render(<GetSubCateOption data={data} />, document.querySelector('#ctrlSubCate'))
                $('#ctrlSubCate').prop('selectedIndex', 0)
                AjaxCommentList()
            }
        })
    })

    // Nav - { ctrlSubCate, ctrlDate, ctrlReport, ctrlShowBan } Change
    $('#ctrlSubCate, #ctrlDate, #ctrlReport, #ctrlShowBan').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        AjaxCommentList()
    })

    // AjaxBox - SearchAuthor
    $('#SearchAuthor').on('click', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#fSort').val(1)
        $('#ctrlSort1').prop('checked', true)
        $('#ctrlCate').prop('selectedIndex', 0)
        $.ajax({
            url: '/BackEndComment/GetSubCateOption',
            type: 'post',
            data: {
                id: $('#ctrlCate').val()
            },
            success: function (data) {
                ReactDOM.render(<GetSubCateOption data={data} />, document.querySelector('#ctrlSubCate'))
                $('#ctrlSubCate').prop('selectedIndex', 0)
            }
        })
        $('#ctrlDate').prop('selectedIndex', 0)
        $('#ctrlReport').prop('selectedIndex', 0)
        $('#ctrlShowBan').prop('selectedIndex', 1)
        $('#searchbox').val(`author: ${$(this).data('email').split('@')[0].toLowerCase()}`)
        $('#fKeyword').val($('#searchbox').val().toLowerCase().trim())
        AjaxCommentList()
        $('#Cancel').click()
    })

    // AjaxBox - BanComment
    $('#BanComment').on('click', function () {
        $('#SearchAuthor').addClass('d-none')
        $('#BanComment').addClass('d-none')
        $('#AjaxBoxTag5-tab').removeClass('d-none')
        ReactDOM.render(
            <BanDetail tag={'comment'} mid={$(this).data('mid')} xid={$(this).data('xid')} />,
            document.querySelector('#AjaxBoxTag5')
        )
        $('#AjaxBoxTag5-tab').tab('show')
        $('#Confirm').removeClass('d-none').off('click').on('click', function () {
            if ($('#BanTaskMessage').val().trim() === '') {
                swal('無法執行', '請填寫系統通知', 'error', {
                    button: false
                })
            } else {
                swal('隱藏評論確認', '是否要隱藏評論？', 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal('隱藏評論進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndComment/BanComment',
                            type: 'post',
                            data: $('#BanTask').serialize(),
                            success: function (data) {
                                swal('隱藏評論成功', data, 'success', {
                                    button: false
                                })
                                AjaxCommentList()
                            },
                            timeout: 20000,
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('隱藏評論終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                        $('#Cancel').click()
                    }
                })
            }
        })
    })

    // AjaxBox - DismissComment
    $('#DismissComment').on('click', function () {
        $('#SearchAuthor').addClass('d-none')
        $('#DismissComment').addClass('d-none')
        $('#AjaxBoxTag5-tab').removeClass('d-none')
        ReactDOM.render(
            <DismissDetail tag={'comment'} mid={$(this).data('mid')} xid={$(this).data('xid')} />,
            document.querySelector('#AjaxBoxTag5')
        )
        $('#AjaxBoxTag5-tab').tab('show')
        $('#Confirm').removeClass('d-none').off('click').on('click', function () {
            if ($('#DismissTaskMessage').val().trim() === '') {
                swal('無法執行', '請填寫系統通知', 'error', {
                    button: false
                })
            } else {
                swal('解除隱藏評論確認', '是否要解除隱藏評論？', 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal('解除隱藏評論進行中', '同步發送通知與站外寄信, 請稍後...', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndComment/DismissComment',
                            type: 'post',
                            data: $('#DismissTask').serialize(),
                            success: function (data) {
                                swal('解除隱藏評論成功', data, 'success', {
                                    button: false
                                })
                                AjaxCommentList()
                            },
                            timeout: 20000,
                            error: function (xmlhttprequest, textstatus, message) {
                                swal('解除隱藏評論終止', textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
                                    button: false
                                })
                            }
                        })
                        $('#Cancel').click()
                    }
                })
            }
        })
    })
})