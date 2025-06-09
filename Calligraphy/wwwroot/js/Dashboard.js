import { fetchImage } from './showImg.js';

$('#DashboardTable').dataTable({
    ajax: {
        url: '/Home/DashboardJson',
        type: 'GET',
        dataSrc: "",  // 如果你的 JSON 根是數組，否則用 dataSrc: "data"
        datatype: 'Json'
    },
    pagingType: "full_numbers",
    pageLength: 10,
    fixedHeader: {
        header: true
    },
    scrollY: 200,
    language: {
        url: 'https://cdn.datatables.net/plug-ins/2.1.5/i18n/zh-HANT.json',
    },
    columns: [
        { data: 'artTitle' },
        {
            data: 'comment',
            render: function (data) {
                //只顯示10個字,如果超過10個字則顯示...
                if (data.length > 10) {
                    return data.substring(0, 10) + '...';
                } else {
                    return data;
                }
            }
        },
        {
            data: 'commentCreate', width: '15%',
            render: function (data) {
                //格式化時間
                if (data) {
                    return new Date(data).toLocaleString('zh-TW', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                        hour: '2-digit',
                        minute: '2-digit',
                        second: '2-digit'
                    });
                }
            }
        },
        {
            data: 'reply',
            render: function (data) {
                //只顯示10個字,如果超過10個字則顯示...
                if (data && data.length > 10) {
                    return data.substring(0, 10) + '...';
                } else {
                    return data || '';
                }
            }
        },
        {
            data: 'dashId',
            render: function (data) {
                return `
                    <div class = "text-nowrap">
                        <a data-id="${data}" class="ReplyBtn btn btn-info" data-bs-toggle="modal" data-bs-target="#ReplyModal">
                            <i class="ti ti-edit" style="font-size:18px;"></i>
                        </a>
                    </div>
                `;
            }
        },
        {
            data: 'artWorkId',
            //不顯示但要抓得到資料
            visible: false,
        }
    ]
});

//動態加載reply內容
$(document).on('click', '.ReplyBtn', async function () {
    var id = $(this).data('id'); // 獲取該筆資料的 ID
    let response = await fetch(`/Home/ReplyPartial/${id}`);
    let partialView = await response.text();  // 讀取返回的 HTML 內容
    $('.Reply').html(partialView);  // 動態插入到指定區域
    // 綁定表單提交事件
    $(document).off('submit', '#ReplyForm').on('submit', '#ReplyForm', async function (e) {
        e.preventDefault(); // 防止默認提交行為

        // 將表單序列化
        let formData = new FormData(this);  // 使用 FormData 獲取表單數據
        // 使用 fetch 發送 POST 請求
        let fetchResponse = await fetch($(this).attr('action'), {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',  // 用來標示這是一個 AJAX 請求
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()  // 防護 token
            }
        });

        if (fetchResponse.ok) {
            let result = await fetchResponse.json(); // 解析為 JSON

            if (result.success) {
                Swal.fire({
                    icon: "success",
                    title: "回覆成功!",
                    showConfirmButton: false,
                    timer: 1500
                });
                $('#ReplyModal').modal('hide');  // 隱藏模態框
                $('#DashboardTable').DataTable().ajax.reload(null, false);  // 刷新表格資料
            } else {
                // 處理失敗，顯示錯誤
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: result.errors ? result.errors.join(", ") : result.message,
                });
            }
        } else {
            // 處理 AJAX 請求錯誤
            Swal.fire({
                icon: "error",
                title: "Oops...",
                text: "請求發生錯誤，請稍後再試",
            });
        }
    });

    // 顯示模態框
    $('#ReplyModal').modal('show');
});

//按讚紀錄table
$('#LikeTable').dataTable({
    ajax: {
        url: '/Home/LikeRecordJson',
        type: 'GET',
        dataSrc: "",  // 如果你的 JSON 根是數組，否則用 dataSrc: "data"
        datatype: 'Json'
    },
    pagingType: "full_numbers",
    pageLength: 10,
    fixedHeader: {
        header: true
    },
    scrollY: 200,
    language: {
        url: 'https://cdn.datatables.net/plug-ins/2.1.5/i18n/zh-HANT.json',
    },
    columns: [
        { data: 'artTitle', width: '10%' },
        {
            data: 'likeCount',
            className: 'text-start',
            width: '10%'
        },
        {
            data: 'artId',
            visible: false
        }
    ]
});

let table = new DataTable('#DashboardTable')
table.on('click', 'tbody tr', async function (e) {
    //如果點的是按鈕或是他的子元素直接return
    if ($(e.target).is('.ReplyBtn') || $(e.target).closest('.ReplyBtn').length > 0) return;

    //取得該row的Id
    let data = table.row(this).data();
    let id = data.artWorkId; // 獲取該筆資料的 ID

    await fetchImage(id, data.artTitle); // 使用 fetchImage 函數來獲取圖片
});

let likeTable = new DataTable('#LikeTable')
likeTable.on('click', 'tbody tr', async function (e) {
    //取得該row的Id
    let data = likeTable.row(this).data();
    let id = data.artId; // 獲取該筆資料的 ID
    await fetchImage(id, data.artTitle); // 使用 fetchImage 函數來獲取圖片
})