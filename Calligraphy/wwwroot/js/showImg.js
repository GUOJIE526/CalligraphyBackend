const imgCache = new Map();// 用來緩存圖片的 Map
export async function fetchImage(id, dataAlt) {
    if (imgCache.has(id)) {
        showImgModel(imgCache.get(id), dataAlt);
        return;
    }
    let response = await fetch(`/Art/ArtWorkImages/${id}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest',
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()  // 防護 token
        },
        body: JSON.stringify(id) // 傳送 ID
    });
    if (response.ok) {
        let result = await response.json();
        imgCache.set(id, result.artImage); // 將圖片 URL 存入快取
        showImgModel(result.artImage, dataAlt); // 顯示圖片模態框
    } else {
        alert('找不到圖片!');
    }
}

function showImgModel(imgUrl, dataAlt) {
    // 顯示圖片的模態框
    $("#PicModal .Picture").empty();
    let img = `<img src="${imgUrl}" alt="${dataAlt}" class="img-fluid" loading="lazy" style="max-width:100%; height:auto;" />`;
    $('.Picture').html(img);
    $('#PicModal').modal('show');
}