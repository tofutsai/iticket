class Pagination extends React.Component {
    gotoPage = x => {
        document.querySelector('#fPageCurrent').value = x
        switch (window.location.pathname) {
            case '/BackEndComment/CommentList':
                AjaxCommentList()
                break
            case '/BackEndArticle/ArticleList':
                AjaxArticleList()
                break
            case '/BackEndMember/MemberList':
                AjaxMemberList()
                break
        }
    }
    gotoNewPage = m => {
        swal({
            title: '請輸入頁碼',
            content: 'input'
        }).then(page => {
            if (page !== null && /^(?=.*[1-9])[0-9]+$/.test(page)) {
                document.querySelector('#fPageCurrent').value = Math.min(parseInt(page), m)
                switch (window.location.pathname) {
                    case '/BackEndComment/CommentList':
                        AjaxCommentList()
                        break
                    case '/BackEndArticle/ArticleList':
                        AjaxArticleList()
                        break
                    case '/BackEndMember/MemberList':
                        AjaxMemberList()
                        break
                }
            }
        })
    }
    render() {
        const p = this.props.page
        const m = this.props.max
        const collection = [1, 2, p-2, p-1, p, p+1, p+2, m-1, m]
        const pages = []
        let flag = false
        for (let i = 1; i <= m; i++) {
            if (collection.includes(i)) {
                pages.push(
                    <li className="page-item">
                        <a onClick={() => this.gotoPage(i)} className="page-link" style={{
                            backgroundColor: `${i === p ? 'lightyellow' : 'transparent'}`,
                        }} href="javascript:">{i}</a>
                    </li>
                )
                flag = false
            } else if (!flag) {
                pages.push(
                    <li className="page-item">
                        <a onClick={() => this.gotoNewPage(m)} className="page-link text-secondary" href="javascript:">...</a>
                    </li>
                )
                flag = true
            }
        }
        return (
            <ul className="pagination">
                <li className={`page-item ${p === 1 ? 'disabled' : ''}`}>
                    <a onClick={() => this.gotoPage(p - 1)} className="page-link" href="javascript:">上一頁</a>
                </li>
                {pages}
                <li className={`page-item ${p === m ? 'disabled' : ''}`}>
                    <a onClick={() => this.gotoPage(p + 1)} className="page-link" href="javascript:">下一頁</a>
                </li>
            </ul>
        )
    }
}

class MemberDetail extends React.Component {
    render() {
        let data = this.props.data
        switch (this.props.tag) {
            case 2:
                return (
                    <div className="table-responsive text-body bg-white">
                        <table className="table table-bordered table-striped mb-0" style={{ width: '100%' }}>
                            <tr>
                                <th>Email</th>
                                <td>{data.MemberEmail}
                                    {
                                        (data.MemberEmail === null || data.MemberEmail === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>姓名</th>
                                <td>{data.MemberName}
                                    {
                                        (data.MemberName === null || data.MemberName === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>身分證字號</th>
                                <td>{data.MemberIDentityNumber}
                                    {
                                        (data.MemberIDentityNumber === null || data.MemberIDentityNumber === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>護照</th>
                                <td>{data.MemberPassport}
                                    {
                                        (data.MemberPassport === null || data.MemberPassport === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>暱稱</th>
                                <td>{data.MemberNickName}
                                    {
                                        (data.MemberNickName === null || data.MemberNickName === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>生日</th>
                                <td>{data.MemberBirthDate}
                                    {
                                        (data.MemberBirthDate === null || data.MemberBirthDate === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>電話</th>
                                <td>{data.MemberPhone}
                                    {
                                        (data.MemberPhone === null || data.MemberPhone === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>獎勵點數</th>
                                <td>{data.MemberPoint}
                                    {
                                        (data.MemberPoint === null || data.MemberPoint === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>地址</th>
                                <td>{data.MemberAddress}
                                    {
                                        (data.MemberAddress === null || data.MemberAddress === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>角色權限</th>
                                <td>{data.MemberRoleName}
                                    {
                                        (data.MemberRoleName === null || data.MemberRoleName === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>性別</th>
                                <td>{data.MemberSex}
                                    {
                                        (data.MemberSex === null || data.MemberSex === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>城市</th>
                                <td>{data.MemberDistrict}
                                    {
                                        (data.MemberDistrict === null || data.MemberDistrict === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                        </table>
                    </div>
                )
                break
            case 3:
                return (
                    <div className="table-responsive text-body bg-white">
                        <table className="table table-bordered table-striped mb-0" style={{ width: '100%' }}>
                            <tr>
                                <th>公司名</th>
                                <td>{data.SellerCompanyName}
                                    {
                                        (data.SellerCompanyName === null || data.SellerCompanyName === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>統編</th>
                                <td>{data.SellerTaxIDNumber}
                                    {
                                        (data.SellerTaxIDNumber === null || data.SellerTaxIDNumber === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>商家網站主頁</th>
                                <td>{data.SellerHomePage}
                                    {
                                        (data.SellerHomePage === null || data.SellerHomePage === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>商家聯絡資訊</th>
                                <td>{data.SellerPhone}
                                    {
                                        (data.SellerPhone === null || data.SellerPhone === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>商家描述資訊</th>
                                <td>{data.SellerDiscription}
                                    {
                                        (data.SellerDiscription === null || data.SellerDiscription === '') &&
                                        <span className="text-danger">未填寫</span>
                                    }
                                </td>
                            </tr>
                            <tr>
                                <th>審核狀態</th>
                                {
                                    data.fPass === '審核通過' &&
                                    <td className="text-success">審核通過</td>
                                }
                                {
                                    data.fPass === '審核不通過' &&
                                    <td className="text-danger">審核不通過</td>
                                }
                                {
                                    data.fPass === '尚未審核' &&
                                    <td className="text-warning">尚未審核</td>
                                }
                            </tr>
                        </table>
                    </div>
                )
                break
            default:
                const baninfo = []
                for (let i = 0; i < data.Reasons.length; i++) {
                    baninfo.push(
                        <tr>
                            <td>{data.Reasons.length - i}</td>
                            <td>{data.Reasons[i]}</td>
                            <td>{data.EndTimes[i]}</td>
                        </tr>
                    )
                }
                return (
                    <div className="table-responsive text-body bg-white">
                        <table className="table table-bordered table-striped mb-0" style={{ width: '100%' }}>
                            <tr>
                                <th>#</th>
                                <th>停權原因</th>
                                <th>結束時間</th>
                            </tr>
                            {baninfo}
                            {
                                data.Reasons.length === 0 &&
                                <tr>
                                    <td colSpan={3}>此會員沒有停權紀錄</td>
                                </tr>
                            }
                        </table>
                    </div>
                )
                break
        }
    }
}

class BanDetail extends React.Component {
    constructor(props) {
        super(props)
        this.state = {
            isShow: false
        }
    }
    render() {
        const tomorrow = `${new Date().getFullYear()}-${String(new Date().getMonth() + 1).padStart(2, '0')}-${String(new Date().getDate() + 1).padStart(2, '0')}`
        return (
            <form id="BanTask" className="p-2">
                <input id="BanTaskType" name="BTtype" type="hidden" value={this.props.tag} />
                <input id="BanTaskXId" name="BTxid" type="hidden" value={this.props.xid} />
                <input id="BanTaskId" name="BTid" type="hidden" value={this.props.mid} />
                <input id="BanTaskMain" name="BTmain" type="hidden" value={
                    this.props.tag === 'comment' ? '您的評論已被隱藏' :
                    this.props.tag === 'article' ? '您的文章已被刪除' :
                    this.props.tag === 'reply' ? '您的回覆已被刪除' : '會員停權通知'
                } />
                <div className="form-group m-1">
                    <label for="BanTaskMessage">
                        {
                            this.props.tag === 'comment' && '隱藏評論並發送系統通知:'
                        }
                        {
                            this.props.tag === 'article' && '刪除文章並發送系統通知:'
                        }
                        {
                            this.props.tag === 'reply' && '刪除回覆並發送系統通知:'
                        }
                        {
                            this.props.tag === 'ban' && '停權會員並發送系統通知:'
                        }
                    </label>
                    <textarea id="BanTaskMessage" name="BTmessage" className="form-control" rows={3}></textarea>
                </div>
                <div className="form-group m-1" style={{ display: this.props.tag === 'ban' ? 'none' : 'block' }}>
                    <div className="form-check d-inline mx-2">
                        <input id="BanTaskBanF" name="BTban" type="radio" value={false} className="form-check-input" style={{ cursor: 'pointer' }} onClick={() => { this.setState({ isShow: false }) }} checked={!this.state.isShow} />
                        <label for="BanTaskBanF" className="form-check-label d-inline" style={{ cursor: 'pointer' }}>
                            {
                                this.props.tag === 'comment' && '只隱藏評論'
                            }
                            {
                                this.props.tag === 'article' && '只刪除文章'
                            }
                            {
                                this.props.tag === 'reply' && '只刪除回覆'
                            }
                        </label>
                    </div>
                    <div className="form-check d-inline mx-2">
                        <input id="BanTaskBanT" name="BTban" type="radio" value={true} className="form-check-input" style={{ cursor: 'pointer' }} onClick={() => { this.setState({ isShow: true }) }} checked={this.state.isShow} />
                        <label for="BanTaskBanT" className="form-check-label d-inline" style={{ cursor: 'pointer' }}>
                            {
                                this.props.tag === 'comment' && '隱藏評論並給予停權處分'
                            }
                            {
                                this.props.tag === 'article' && '刪除文章並給予停權處分'
                            }
                            {
                                this.props.tag === 'reply' && '刪除回覆並給予停權處分'
                            }
                        </label>
                    </div>
                </div>
                <div className="form-group m-1" style={{ visibility: this.props.tag === 'ban' ? 'visible' : this.state.isShow ? 'visible' : 'hidden' }}>
                    <label for="BanTaskEndTime">停權到期日:</label>
                    <input id="BanTaskEndTime" name="BTendtime" defaultValue={tomorrow} type="date" className="form-control" min={tomorrow} />
                </div>
            </form>
        )
    }
}

class DismissDetail extends React.Component {
    constructor(props) {
        super(props)
        this.state = {
            isShow: false
        }
    }
    render() {
        return (
            <form id="DismissTask" className="p-2">
                <input id="DismissTaskXId" name="DTxid" type="hidden" value={this.props.xid} />
                <input id="DismissTaskId" name="DTid" type="hidden" value={this.props.mid} />
                <input id="DismissTaskMain" name="DTmain" type="hidden" value="您的評論已被重新顯示" />
                <div className="form-group m-1">
                    <label for="DismissTaskMessage">解除隱藏評論並發送系統通知:</label>
                    <textarea id="DismissTaskMessage" name="DTmessage" className="form-control" rows={3}></textarea>
                </div>
                <div className="form-group m-1">
                    <div className="form-check d-inline mx-2">
                        <input id="DismissTaskBanF" name="DTunban" type="radio" value={false} className="form-check-input" style={{ cursor: 'pointer' }} onClick={() => { this.setState({ isShow: false }) }} checked={!this.state.isShow} />
                        <label for="DismissTaskBanF" className="form-check-label d-inline" style={{ cursor: 'pointer' }}>
                            解除隱藏評論
                        </label>
                    </div>
                    <div className="form-check d-inline mx-2">
                        <input id="DismissTaskBanT" name="DTunban" type="radio" value={true} className="form-check-input" style={{ cursor: 'pointer' }} onClick={() => { this.setState({ isShow: true }) }} checked={this.state.isShow} />
                        <label for="DismissTaskBanT" className="form-check-label d-inline" style={{ cursor: 'pointer' }}>
                            解除隱藏評論並解除停權處分
                        </label>
                    </div>
                </div>
                <p className="text-warning m-1" style={{ visibility: this.state.isShow ? 'visible' : 'hidden' }}>
                    <i className="fas fa-exclamation-triangle mr-1"></i> 停權的原因有很多種, 請斷定是否真的要解除停權！
                </p>
            </form>
        )
    }
}

function keyInput() {
    $('#fKeyword').val($('#searchbox').val().toLowerCase().trim())
    $('#fPageCurrent').val(1)
    $('#fPageSize').val(10)
    $('#pageAmount').prop('selectedIndex', 0)
    switch (window.location.pathname) {
        case '/BackEndComment/CommentList':
            AjaxCommentList()
            break
        case '/BackEndArticle/ArticleList':
            AjaxArticleList()
            break
        case '/BackEndMember/MemberList':
            $('#fSort').val(0)
            MemberRoleInfoFont($('#fSort').val())
            AjaxMemberList()
            break
    }
}

function keyHighlight(text, keyword) {
    text = htmlSpecialChars(text)
    if (keyword.length) {
        let i = 0
        let flag = true
        const prefix = []
        const suffix = []
        while (i < text.length) {
            if (text.toLowerCase().slice(i).search('&') === 0) {
                flag = false
            }
            if (flag && text.toLowerCase().slice(i).search(keyword) === 0) {
                prefix.push(i)
                suffix.push(i + keyword.length)
                i += keyword.length
            } else {
                i += 1
            }
            if (!flag && text.toLowerCase().slice(i).search(';') === 0) {
                flag = true
                i += 1
            }
        }
        while (prefix.length) {
            let iS = suffix.pop()
            text = `${text.slice(0, iS)}</span>${text.slice(iS)}`
            let iP = prefix.pop()
            text = `${text.slice(0, iP)}<span style="background-color: springgreen">${text.slice(iP)}`
        }
    }
    return text
}

function htmlSpecialChars(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, x => map[x]);
}

function MemberRoleInfoFont(value) {
    const fonts = [$('#listHead1 span'), $('#listHead2 span'), $('#listHead3 span'), $('#listHead4 span')]
    for (let font of fonts) {
        font.html('<i class="fas fa-sort"></i>').closest('th').css('color', 'black')
    }
    switch (value) {
        case '1a':
            fonts[0].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            break
        case '1d':
            fonts[0].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            break
        case '2a':
            fonts[1].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            break
        case '2d':
            fonts[1].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            break
        case '3a':
            fonts[2].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            break
        case '3d':
            fonts[2].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            break
        case '4a':
            fonts[3].html('<i class="fas fa-sort-down"></i>').closest('th').css('color', 'orangered')
            $('#ctrlBtn5').removeClass('d-none')
            break
        case '4d':
            fonts[3].html('<i class="fas fa-sort-up"></i>').closest('th').css('color', 'orangered')
            break
    }
}