var toggledButtons = [];

var filesoffcanvas = document.getElementById("navbarToggleExternalContent");

var filesCounter = document.getElementById("files-counter")

var leaveButton = document.getElementById("leave-button")

var timeCounter = document.getElementById("time-counter")

let filesoffcanvasObj = 0;

if (timeCounter) {
    var currentTime = timeToSeconds(timeCounter.innerHTML);

    setInterval(function () {
        currentTime -= 1;

        timeCounter.innerHTML = secondsToTime(currentTime);

        if (currentTime < 0) {
            window.location.href = "/" 
        }

    }, 1000);
}
    

if (leaveButton)
leaveButton.addEventListener("click", function () {
    var request = new XMLHttpRequest();

    request.onreadystatechange = function () {
        if (request.readyState == 4) {
            window.location.replace("/")
        }
    }

    request.open("delete", window.location.href);

    request.send()
});

document.addEventListener('DOMContentLoaded', function () {


    var files = document.querySelectorAll(".file");

    files.forEach(file => {
        file.addEventListener('click', function (event) {
            if (event.target.closest(".file").classList.contains("add-file-button")) {
                return;
            }


            new window.bootstrap.Button(file).toggle();
            if (event.target.closest(".file").getAttribute("fileid")) {
                toggleArrayElement(toggledButtons, event.target.closest(".file").getAttribute("fileid"))
            }

            if (filesCounter) {
                filesCounter.innerText = "Выбрано " + toggledButtons.length + " Файлов";
            }

            if (!filesoffcanvasObj && filesoffcanvas) {
                filesoffcanvasObj = new window.bootstrap.Collapse(filesoffcanvas)
            }

            if (filesoffcanvas) {
                if (toggledButtons.length > 0) {
                    filesoffcanvasObj.show();
                } else {
                    filesoffcanvasObj.hide();
                }
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

function timeToSeconds(time) {
    const [hours, minutes, seconds] = time.split(':').map(Number);
    return hours * 3600 + minutes * 60 + seconds;
}

function secondsToTime(seconds) {
    const hours = Math.floor(seconds / 3600).toString().padStart(2, '0');
    const minutes = Math.floor((seconds % 3600) / 60).toString().padStart(2, '0');
    const secs = (seconds % 60).toString().padStart(2, '0');
    return `${hours}:${minutes}:${secs}`;
}