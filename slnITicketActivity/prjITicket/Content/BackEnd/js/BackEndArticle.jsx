function AjaxArticleList() {
    $('#isHandle').removeClass('d-none')
    $.ajax({
        url: '/BackEndArticle/ArticleList',
        type: 'post',
        data: $('#ctrlForm').serialize(),
        success: function (data) {
            if (data[0].ChangePage != 0) {
                $('#fPageCurrent').val(data[0].ChangePage)
            }
            $('#ctrlHint').text($('#fKeyword').val() == '' ? ''
                : $('#fKeyword').val().startsWith('author:') ? `查詢作者: ${$('#fKeyword').val().split(':')[1].trim()}`
                : `關鍵字: ${$('#fKeyword').val()}`)
            ReactDOM.render(<ArticleList data={data.slice(1)} />, document.querySelector('#listBody'))
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

class ArticleDetail extends React.Component {
    render() {
        let data = this.props.data
        return (
            <div className="row">
                <div className={`${this.props.isArticle ? 'col-9 pl-3 pr-1 py-3' : 'col pl-3 pr-1 py-3'}`}>
                    <div className="card text-body" style={{ height: '100%' }}>
                        <div className="card-header">
                            <h5>{data[0].MemberEmail.split('@')[0]} 的{this.props.isArticle ? '文章' : '回覆'}內容</h5>
                        </div>
                        <div className="card-body">
                            {
                                this.props.isArticle &&
                                <h3>
                                    <a href={`/Forum/forum_content?articleID=${data[0].ArticleID}`} target="_blank">
                                        {data[0].ArticleTitle}
                                    </a>
                                </h3>
                            }
                            {
                                !this.props.isArticle &&
                                <h4>
                                    <a href={`/Forum/forum_content?articleID=${data[0].ReplyArticleID}`} target="_blank">
                                        Re: {data[0].ReplyArticleTitle}
                                    </a>
                                </h4>
                            }
                            <article id="articleView" className="mt-1" dangerouslySetInnerHTML={{ __html: data[0].XContent }}></article>
                        </div>
                    </div>
                </div>
                <div className={`${this.props.isArticle ? 'col-3 pl-1 pr-2 py-3' : 'col pl-1 pr-2 py-3'}`}>
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
                                <span className="text-secondary">{this.props.isArticle ? '文章' : '回覆'}沒有被檢舉</span>
                            }
                        </div>
                    </div>
                </div>
            </div>
        )
    }
}

class ArticleList extends React.Component {
    handleMouseEnter = x => {
        x.style.backgroundColor = 'lightyellow'
        x.querySelector('td:first-child').innerHTML = '<i class="far fa-dot-circle"></i>'
    }
    handleMouseLeave = x => {
        x.style.backgroundColor = 'white'
        x.querySelector('td:first-child').innerHTML = '<i class="far fa-circle"></i>'
    }
    handleClick = (id, isarticle) => {
        let isArticle = isarticle === 'true'
        $('#AjaxBoxTag3-tab').addClass('d-none')
        $('#AjaxBoxTag5-tab').addClass('d-none')
        $('#SearchAuthor').removeClass('d-none')
        $('#DeleteArticleOrReply').removeClass('d-none')
        $('#Confirm').addClass('d-none')
        $.ajax({
            url: '/BackEndArticle/ArticleDetail',
            type: 'post',
            data: {
                id: id,
                isArticle: isArticle
            },
            success: function (data) {
                if (isArticle) {
                    $('#AjaxBox>div').removeClass('modal-lg').addClass('modal-xl')
                    $('#SearchAuthor').html('<i class="fas fa-search mr-1"></i> 查詢作者所有文章')
                    $('#DeleteArticleOrReply').html('<i class="fas fa-skull-crossbones mr-1"></i> 我覺得不行 (刪除文章)')
                } else {
                    $('#AjaxBox>div').removeClass('modal-xl').addClass('modal-lg')
                    $('#SearchAuthor').html('<i class="fas fa-search mr-1"></i> 查詢作者所有回覆')
                    $('#DeleteArticleOrReply').html('<i class="fas fa-skull-crossbones mr-1"></i> 我覺得不行 (刪除回覆)')
                }
                ReactDOM.render(<ArticleDetail isArticle={isArticle} data={data} />, document.querySelector('#AjaxBoxTag1'))
                $('#articleView img').addClass('img-fluid')
                $('#articleView a').on('click', function (e) {
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
                ReactDOM.render(<MemberDetail tag={2} data={data[0]} />, document.querySelector('#AjaxBoxTag2'))
                ReactDOM.render(<MemberDetail tag={3} data={data[0]} />, document.querySelector('#AjaxBoxTag3'))
                ReactDOM.render(<MemberDetail tag={0} data={data[0]} />, document.querySelector('#AjaxBoxTag4'))
                if (data[0].SellerID !== null) {
                    $('#AjaxBoxTag3-tab').removeClass('d-none')
                }
                $('#AjaxBoxTag1-tab').tab('show')
                $('#SearchAuthor').data('email', data[0].MemberEmail)
                $('#DeleteArticleOrReply').data('mid', data[0].MemberID).data('xid', data[0].ArticleID || data[0].ReplyID)
                    .data('tag', data[0].ArticleID !== null ? 'article' : 'reply')
                $('#AjaxBox').modal({ show: true })
            }
        })
    }
    handleError = x => {
        x.src = 'https://via.placeholder.com/600x200?text=ITicket'
    }
    render() {
        let fKeyword = document.querySelector('#fKeyword').value
        return (
            <React.Fragment>
                {this.props.data.map(e =>
                    <tr data-id={e.ARxID} data-isarticle={e.IsArticle} style={{ cursor: 'pointer' }}
                        onMouseEnter={(e) => this.handleMouseEnter(e.currentTarget)}
                        onMouseLeave={(e) => this.handleMouseLeave(e.currentTarget)}
                        onClick={(e) => this.handleClick(e.currentTarget.dataset.id, e.currentTarget.dataset.isarticle)}
                    >
                        <td className="pt-2 pb-1"><i className="far fa-circle"></i></td>
                        <td className="pt-2 pb-1">
                            <img src={`${e.ARxPicture}`} className="img-fluid img-thumbnail" onError={(x) => this.handleError(x.currentTarget)} />
                        </td>
                        <td className="pt-2 pb-1">
                            <span dangerouslySetInnerHTML={{ __html: keyHighlight(e.ARxAuthor.split('@', 2)[0], fKeyword) }}></span>
                            <br />
                            <sup className="text-secondary">&nbsp;&nbsp;&nbsp;@{e.ARxAuthor.split('@', 2)[1]}</sup>
                        </td>
                        <td className="pt-2 pb-1" dangerouslySetInnerHTML={{ __html: keyHighlight(`${e.IsArticle ? '' : 'Re: '}${e.ARxArticle}`, fKeyword) }}></td>
                        <td className="pt-2 pb-1">{e.ARxDate.split(' ')[0]}&nbsp;&nbsp;&nbsp;{e.ARxDate.split(' ')[1]}</td>
                        <td className="pt-2 pb-1">{e.ARxReportCount}</td>
                    </tr>
                )}
            </React.Fragment>
        )
    }
}

$(function () {
    // DOMContentLoaded
    AjaxArticleList()

    // Basic - PageSize Change
    $('#pageAmount').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val($(this).val())
        AjaxArticleList()
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
        AjaxArticleList()
    })

    // Nav - { ctrlCate, ctrlDate, ctrlReport, ctrlType } Change
    $('#ctrlCate, #ctrlDate, #ctrlReport, #ctrlType').on('change', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        AjaxArticleList()
    })

    // AjaxBox - SearchAuthor
    $('#SearchAuthor').on('click', function () {
        $('#fPageCurrent').val(1)
        $('#fPageSize').val(10)
        $('#pageAmount').prop('selectedIndex', 0)
        $('#fSort').val(1)
        $('#ctrlSort1').prop('checked', true)
        $('#ctrlCate').prop('selectedIndex', 0)
        $('#ctrlDate').prop('selectedIndex', 0)
        $('#ctrlReport').prop('selectedIndex', 0)
        $('#searchbox').val(`author: ${$(this).data('email').split('@')[0].toLowerCase()}`)
        $('#fKeyword').val($('#searchbox').val().toLowerCase().trim())
        AjaxArticleList()
        $('#Cancel').click()
    })

    // AjaxBox - DeleteArticleOrReply
    $('#DeleteArticleOrReply').on('click', function () {
        $('#SearchAuthor').addClass('d-none')
        $('#DeleteArticleOrReply').addClass('d-none')
        $('#AjaxBoxTag5-tab').removeClass('d-none')
        ReactDOM.render(
            <BanDetail tag={$(this).data('tag')} mid={$(this).data('mid')} xid={$(this).data('xid')} />,
            document.querySelector('#AjaxBoxTag5')
        )
        $('#AjaxBoxTag5-tab').tab('show')
        let tagInfo = $(this).data('tag') === 'article' ? '文章' : '回覆'
        $('#Confirm').removeClass('d-none').off('click').on('click', function () {
            if ($('#BanTaskMessage').val().trim() === '') {
                swal('無法執行', '請填寫系統通知', 'error', {
                    button: false
                })
            } else {
                swal(`刪除${tagInfo}確認`, `是否要刪除${tagInfo}？`, 'warning', {
                    buttons: {
                        cancel: '取消',
                        confirm: '確定'
                    }
                }).then(x => {
                    if (x) {
                        swal(`刪除${tagInfo}進行中`, '同步發送通知與站外寄信, 請稍後...', 'info', {
                            button: false,
                            closeOnClickOutside: false,
                            closeOnEsc: false,
                        })
                        $.ajax({
                            url: '/BackEndArticle/DeleteArticleOrReply',
                            type: 'post',
                            data: $('#BanTask').serialize(),
                            success: function (data) {
                                swal(`刪除${tagInfo}成功`, data, 'success', {
                                    button: false
                                })
                                AjaxArticleList()
                            },
                            timeout: 20000,
                            error: function (xmlhttprequest, textstatus, message) {
                                swal(`刪除${tagInfo}終止`, textstatus === 'timeout' ? '超時: 超過了 20 秒' : textstatus, 'error', {
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