var label = document.querySelector('.custum-file-upload');
var text = document.querySelector('.text');
var icon = document.querySelector('.icon');
var picbody = document.querySelector('.picbody');
var magnifier = document.querySelector('.magnifier');
var submit = document.querySelector('.submitBtn');
var inputbox = document.querySelectorAll('input');

//inputbox[0]是圖檔 上傳後可以顯示圖片
inputbox[0].addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        //檢查是否為圖片檔
        const allowedExtensions = ['jpg', 'jpeg', 'png', 'bmp', 'gif', 'heic'];
        const fileExtension = file.name.split('.').pop().toLowerCase();
        if (!allowedExtensions.includes(fileExtension)) {
            alert('請上傳有效的圖片檔案（jpg, jpeg, png, bmp, gif, heic）');
            e.target.value = ''; // 清空選擇的檔案
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            const img = new Image();
            img.onload = function () {
                picbody.innerHTML = '<img src="' + e.target.result + '" class="img-fluid" alt="Responsive image">';
                label.style.backgroundImage = 'url(' + e.target.result + ')';
                label.style.backgroundSize = 'cover';
                label.style.backgroundPosition = 'center center';
                text.style.display = 'none';
                icon.style.display = 'none';
                magnifier.style.display = 'flex';
                magnifier.style.justifyContent = 'center';
                magnifier.style.alignItems = 'center';
            }
            img.src = e.target.result;
        };
        reader.readAsDataURL(file);
    } else {
        label.style.backgroundImage = 'none';
        text.style.display = 'block';
        icon.style.display = 'block';
        magnifier.style.display = 'none';
        picbody.innerHTML = '';
    }
});
// Delete button functionality
const deleteButton = document.querySelector('.delete');
deleteButton.addEventListener('click', function () {
    label.style.backgroundImage = 'none';
    text.style.display = 'block';
    icon.style.display = 'block';
    inputbox[0].value = '';
    magnifier.style.display = "none";
});
