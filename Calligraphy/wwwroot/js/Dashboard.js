import { fetchImage } from './showImg.js';

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
    scrollY: 250,
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


let likeTable = new DataTable('#LikeTable')
likeTable.on('click', 'tbody tr', async function (e) {
    //取得該row的Id
    let data = likeTable.row(this).data();
    let id = data.artId; // 獲取該筆資料的 ID
    await fetchImage(id, data.artTitle); // 使用 fetchImage 函數來獲取圖片
})