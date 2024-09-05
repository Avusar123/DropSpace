var toggledButtons = [];

document.addEventListener('DOMContentLoaded', function () {
    var files = document.querySelectorAll(".file");

    files.forEach(file => {
        file.addEventListener('click', function (event) {
            new window.bootstrap.Button(file).toggle();
            if (event.target.closest(".file").getAttribute("fileid")) {
                toggleArrayElement(toggledButtons, event.target.closest(".file").getAttribute("fileid"))
            }
        });
    })
});

function toggleArrayElement(arr, element) {
    const index = arr.indexOf(element);
    if (index === -1) {
        // Если элемент не найден, добавляем его
        arr.push(element);
    } else {
        // Если элемент найден, удаляем его
        arr.splice(index, 1);
    }
    return arr;
}