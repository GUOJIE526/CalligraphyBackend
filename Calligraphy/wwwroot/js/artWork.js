import { fetchImage } from './showImg.js';

let table = $('#ArtTable').DataTable({
    ajax: {
        url: '/Home/AllArtworkJson',
        type: 'GET',
        dataSrc: "",  // 如果你的 JSON 根是數組，否則用 dataSrc: "data"
        datatype: 'Json'
    },
    pagingType: "full_numbers",
    fixedHeader: {
        header: true
    },
    language: {
        url: 'https://cdn.datatables.net/plug-ins/2.1.5/i18n/zh-HANT.json',
    },
    columns: [
        { data: 'title' },
        { data: 'views'},
        {
            data: 'description',
            render: function (data) {
                //只顯示10個字,如果超過10個字則顯示...
                if (!data) return '';
                if (data.length > 10) {
                    return data.substring(0, 10) + '...';
                } else {
                    return data;
                }
            }
        },
        {
            data: 'createdYear',
            render: function (data) {
                //格式化時間
                if (data) {
                    return new Date(data).toLocaleString('zh-TW', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                    });
                }
            }
        },
        { data: 'style' },
        { data: 'material' },
        { data: 'size' },
        {
            data: 'isVisible',
            render: function (data, type, row) {
                //以checkbox顯示bool值
                const checked = data ? 'checked' : '';
                return `<label class="switch">
                          <input type="checkbox" class="toggle-visible" data-id="${row.artWorkId}" ${checked}>
                          <span class="slider"></span>
                        </label>`
            }
        },
        {
            data: 'artWorkId',
            render: function (data) {
                return `
                    <div class = "text-nowrap">
                        <a data-id="${data}" class="EditBtn btn btn-info" data-bs-toggle="modal" data-bs-target="#EditModal">
                            <i class="ti ti-edit" style="font-size:18px;"></i>
                        </a>
                        <a data-id="${data}" class="deleteBtn btn btn-danger">
                            <i class="ti ti-trash" style="font-size:18px;"></i>
                        </a>
                    </div>
                `;
            }
        }
    ]
});

$(document).on('change', '.toggle-visible', async function () {
    const artWorkId = $(this).data('id'); // 獲取該筆資料的 ID
    const isChecked = $(this).is(':checked'); // 獲取 checkbox 的狀態
    let response = await fetch('/Home/ToggleIsVisible', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest',  // 用來標示這是一個 AJAX 請求
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()  // 防護 token
        },
        body: JSON.stringify({ ArtWorkId: artWorkId, IsVisible: isChecked }), // 將資料轉為 JSON 字串
    });
    if (!response.ok) {
        alert("顯示更新失敗");
        $('.toggle-visible').prop('checked', !isChecked); // 還原 checkbox 狀態
    }
})

table.on('click', 'tbody tr', async function (e) {
    //如果點的是按鈕或是他的子元素直接return
    if ($(e.target).closest('.EditBtn').length > 0 || $(e.target).closest('.switch').length > 0 || $(e.target).closest('.deleteBtn').length > 0) return;

    //取得該row的Id
    let data = table.row(this).data();
    let id = data.artWorkId; // 獲取該筆資料的 ID

    await fetchImage(id, data.title); // 使用 showImg.js 中的函數來顯示圖片
});

//動態加載reply內容
$(document).on('click', '.EditBtn', async function () {
    var id = $(this).data('id'); // 獲取該筆資料的 ID
    let response = await fetch(`/Home/ArtWorkPartial/${id}`);
    let partialView = await response.text();  // 讀取返回的 HTML 內容
    $('.Edit').html(partialView);  // 動態插入到指定區域
    // 綁定表單提交事件
    $(document).off('submit', '#EditForm').on('submit', '#EditForm', async function (e) {
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
                    title: "作品更新成功!",
                    showConfirmButton: false,
                    timer: 1500
                });
                $('#EditModal').modal('hide');  // 隱藏模態框
                $('#ArtTable').DataTable().ajax.reload(null, false);  // 刷新表格資料
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
    $('#EditModal').modal('show');
});

$(document).on('click', '.deleteBtn', async function () {
    let id = $(this).data('id'); // 獲取該筆資料的 ID
    Swal.fire({
        title: '確定要刪除這個作品嗎?',
        text: "刪除後將無法恢復!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        confirmButtonText: '確定',
        cancelButtonColor: '#d33',
    }).then(async (result) => {
        if (result.isConfirmed) {
            let response = await fetch(`/Home/DeleteArtWork/${id}`, {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',  // 用來標示這是一個 AJAX 請求
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()  // 防護 token
                }
            });
            if (response.ok) {
                Swal.fire({
                    icon: "success",
                    title: "作品已刪除!",
                    showConfirmButton: false,
                    timer: 1500
                });
                $('#ArtTable').DataTable().ajax.reload(null, false);  // 刷新表格資料
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: "刪除失敗，請稍後再試",
                });
            }
        }
    });
});