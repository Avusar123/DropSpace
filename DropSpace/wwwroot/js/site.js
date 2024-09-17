var toggledFiles = [];

const filesoffcanvas = document.getElementById("filesActionsNavbar");

const filesCounter = document.getElementById("files-counter")

const hostName = window.location.href.split("/")[2];

const leaveButton = document.getElementById("leave-button")

const timeCounter = document.getElementById("time-counter")

const sessionsContainer = document.getElementById("sessionsContainer");

const sessionsCounter = document.getElementById("sessionsCounter");

const errorToastEl = document.getElementById("error-toast");

let errorToast;

let qrcode;

let inviteToast;

let successToast;

if (errorToastEl) {
    errorToast = new bootstrap.Toast(errorToastEl);
}

let filesoffcanvasObj = 0;

let code;

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/Session/Subscribe")
    .build();

connection.on("NewInvite", function (name, id) {
    $("#invite-toast-content").text('Вас приглашают в сессию "' + name + '"');
    $('#invite-toast-button').on("click", () => {
        window.location.href = window.location.href + "Session/" + id
    })
    inviteToast.show();
})

connection.on("NewChunkUploaded", function (pendingUpload) {
    if (!document.querySelector(".file-container")) {
        return;
    }

    var file = $(`.file[fileid=${pendingUpload.id}]`)

    file.find(".sended-size").text(pendingUpload.sendedSizeMb)
    file.find(".progress-bar").css("width", `${(pendingUpload.sendedSize / pendingUpload.size) * 100}%`)

    countSesisonSize();
})

connection.on("FileListChanged", function () {
    updateFiles(connection);
})

connection.on("NewUser", function (session) {
    var toast = new window.bootstrap.Toast($("#primary-toast"));
    $("#primary-toast-body").text("К сессии присоединился новый пользователь!")
    $("#memberCount").text(session.membersCount);
    toast.show();
})
connection.on("UserLeft", function (session) {
    var toast = new window.bootstrap.Toast($("#primary-toast"));
    $("#primary-toast-body").text("Пользователь покинул сессию!")
    $("#memberCount").text(session.membersCount);
    toast.show();
})

connection.on("SessionExpired", function () {
    updateSessions(connection)
})

connection.on("ErrorRecieved", function (err) {
    showError(err)
})

connection.start().catch(function (err) {
    return console.error(err.toString());
}).then(() => {
    updateSessions(connection);
    updateFiles(connection);
    code = refreshCode(connection).then((result) => {
        if (document.querySelector("#home-qr")) {
            var currentUrl = window.location.href + "Session/Invite?code=" + result;
            qrcode = new QRCode(document.querySelector("#home-qr"), {
                text: currentUrl,
                width: 256, // Ширина QR-кода
                height: 256 // Высота QR-кода
            });
        }
    });
});

async function refreshCode(connection) {
    var result = await connection.invoke("RefreshCode") 
    $("#invite-code").text(result);
    return result;
}

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
    const request = new XMLHttpRequest();

    request.onreadystatechange = function (event) {
        if (request.status == 200 && request.readyState == 4) {
            window.location.replace("/")
        }

        if (request.status == 400) {
            showError("Неизвестная ошибка!")
        }
    }

    request.open("delete", window.location.href);

    request.send()
});

const boxes = document.querySelectorAll('.code-input');

function showError(text) {
    $("#error-toast-body").text(text);
    errorToast.show()
}

const inviteUserButton = document.querySelector("#inviteUser");

const inviteBody = document.querySelector("#invite-body")

if (inviteUserButton)
inviteUserButton.addEventListener("click", function (e) {
    e.preventDefault();
    const url = window.location.href;
    const SessionId = url.split('/').pop();
    connection.invoke("SendInviteByCode", collectCode(boxes), SessionId).then((res) => {
        if (res) {
            successToast.show();
        }
    })
})

document.addEventListener('DOMContentLoaded', function () {
    var invtoast = document.querySelector("#invite-toast")

    var succstoast = document.querySelector("#success-toast");

    if (succstoast)
        successToast = new window.bootstrap.Toast(succstoast)

    if (invtoast)
        inviteToast = new window.bootstrap.Toast(invtoast);

    const container = document.querySelector('.code-input-container');

    if (container) {
        const submitBtn = document.querySelector("#" + container.getAttribute("data-button"))

        boxes.forEach((box, index) => {
            box.addEventListener('input', () => {
                if (box.value.length === box.maxLength && index < boxes.length - 1) {
                    boxes[index + 1].focus();
                }

                toggleSubmitButton(submitBtn);
            });

            box.addEventListener('keydown', (event) => {
                if (event.key === 'Backspace' && box.value === '' && index > 0) {
                    boxes[index - 1].focus();
                }

                toggleSubmitButton(submitBtn);
            });
        });
    }

    $("#share-button").on("click", function (event) {

        if (qrcode)
            return;

        var currentUrl = window.location.href;

        // Генерируем QR-код
        qrcode = new QRCode(document.getElementById("qrcode"), {
            text: currentUrl,
            width: 256, // Ширина QR-кода
            height: 256 // Высота QR-кода
        });
    })

    var createSessionButton = document.getElementById("createSessionButton");

    if (createSessionButton)
    createSessionButton.addEventListener("click", function (event) {
        event.preventDefault();

        const request = new XMLHttpRequest();

        request.onreadystatechange = function () {
            if (request.status == 200 && request.readyState == 4) {
                window.location.href = "/Session/" + JSON.parse(request.response).id
            }

            if (request.status == 400 && request.readyState == 4) {
                showError(Object.values(JSON.parse(request.response))[0]);
            }
        }

        let formData = new FormData(document.getElementById("createSessionForm"));

        request.open("post", "/Session");

        request.send(formData);
    })

    async function onFileInputChange(ev) {
        const url = window.location.href;
        const SessionId = url.split('/').pop();

        for (var i = 0; i < ev.target.files.length; i++) {
            
            const file = ev.target.files[i]

            const arrayBuffer = await file.arrayBuffer();
            const hashBuffer = await crypto.subtle.digest('SHA-256', arrayBuffer);
            const hashArray = Array.from(new Uint8Array(hashBuffer));
            const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

            var formData = new FormData();

            formData.append("size", file.size);
            formData.append("hash", hashHex);
            formData.append("name", file.name);
            formData.append("sessionId", SessionId)

            var rowresponse = await fetch("https://" + hostName + "/File", {
                method: 'POST',
                body: formData
            });

            if (!rowresponse.ok) {
                showError(await rowresponse.text())
                continue;
            }

            response = await rowresponse.json();
            

            var newFormdata

            var uploadId;

            while (!response.isCompleted) {

                if (!uploadId) {
                    uploadId = response.id;
                }

                newFormdata = new FormData();

                newFormdata.append("file", sliceFile(file, response.sendedSize, response.chunkSize))

                newFormdata.append("uploadId", uploadId)

                rowresponse = await fetch("https://" + hostName + "/File", {
                    method: 'PUT',
                    body: newFormdata
                });

                if (!rowresponse.ok) {
                    showError(await rowresponse.text())
                    break;
                }

                response = await rowresponse.json();
            }

            uploadId = null;
        }
    }

    function sliceFile(file, start, chunkSize) {

        const end = Math.min(start + chunkSize, file.size);

        // Получаем текущий кусок файла
        const chunk = file.slice(start, end);
        // Используем slice для получения среза
        return chunk;
    }

    $("#fileInput").on("change", function (ev) {
        onFileInputChange(ev)
    })

    var files = document.querySelectorAll(".file");

    files.forEach(file => {
        file.addEventListener('click', function (event) {
            if (event.target.closest(".file").classList.contains("add-file-button")) {
                document.querySelector("#fileInput").click();
                return;
            }

            new window.bootstrap.Button(file).toggle();
            if (event.target.closest(".file").getAttribute("fileid")) {
                toggleArrayElement(toggledFiles, event.target.closest(".file").getAttribute("fileid"))
            }

            if (filesCounter) {
                filesCounter.innerText = "Выбрано " + toggledFiles.length + " Файлов";
            }

            if (!filesoffcanvasObj && filesoffcanvas) {
                filesoffcanvasObj = new window.bootstrap.Collapse(filesoffcanvas)
            }

            if (filesoffcanvas) {
                if (toggledFiles.length > 0) {
                    filesoffcanvasObj.show();
                } else {
                    filesoffcanvasObj.hide();
                }
            }
        });
    })
});

if (document.querySelector("#file-delete"))
document.querySelector("#file-delete").addEventListener("click", async function (ev) {
    ev.preventDefault();

    const sessionId = window.location.href.split('/').pop();

    toggledFiles.forEach(async (fileId, index) => {

        var formData = new FormData();

        formData.append("sessionId", sessionId);

        formData.append("fileId", fileId)

        responce = await fetch(`/File/`, {
            method: 'DELETE',
            body: formData
        })

        if (index == toggledFiles.length - 1) {
            toggledFiles = [];

            filesoffcanvasObj.hide();
        }
    })
})

if (document.querySelector("#file-download"))
document.querySelector("#file-download").addEventListener("click", async function (ev) {
    ev.preventDefault();

    const sessionId = window.location.href.split('/').pop();

    toggledFiles.forEach(async (fileId, index) => {

        let response;

        let fileData = [];

        let start = 0;

        fileResponse = await fetch(`/File?fileId=${fileId}`, {
            method: 'GET'
        })

        const fileObj = await fileResponse.json();

        do {
            var formData = new FormData();

            formData.append("sessionId", sessionId);

            formData.append("fileId", fileId)

            formData.append("startWith", start)

            response = await fetch(`/File/Download`, {
                method: 'POST',
                body: formData
            })

            const chunk = await response.blob();
            fileData.push(chunk);
            start += chunk.size;

        } while (start < fileObj.size)

        const blob = new Blob(fileData);
        const downloadUrl = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = downloadUrl;
        a.download = fileObj.fileName;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(downloadUrl);
        a.remove();

        document.querySelector(`*[fileId="${fileId}"]`).click();
    })
})


function countSesisonSize() {
    if (!document.querySelector("#session-size")) {
        return;
    }

    var sum = 0;

    $(".sended-size").each(function (ind, size) {
        var value = parseFloat(size.innerText); // Получаем текст из каждого <span> и преобразуем в число
        sum += value; // Добавляем к общей сумме
    });

    $("#session-size").text(sum);
}

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

function toggleSubmitButton(submitBtn) {
    const allFilled = Array.from(boxes).every(box => box.value.length === 1);
    submitBtn.disabled = !allFilled;
}

async function updateSessions(connection) {

    const sessions = await connection.invoke("GetSessions");

    if (!sessionsContainer)
        return;

    if (sessionsCounter) {
        sessionsCounter.innerHTML = sessions.length
    }

    sessionsContainer.innerHTML = "";

    sessions.forEach((session) => {
        const li = document.createElement("li");
        const a = document.createElement("a");
        a.className = "btn btn-dark w-100";
        a.style = "text-align: left";
        a.href = "/Session/" + session.id;
        a.text = session.name;
        li.appendChild(a);
        li.className = "nav-item"

        if (inviteBody) {
            a.addEventListener("click", function (ev) {
                ev.preventDefault();

                const urlParams = new URLSearchParams(window.location.search);

                // Извлекаем параметр 'code'
                const code = urlParams.get('code');1

                var params = ev.target.getAttribute("href").split("/");

                var sessionId = params[params.length - 1]

                connection.invoke("SendInviteByCode", code, sessionId).then((res) => {
                    if (res) {
                        window.location.href = "/";
                    }
                })
            })
        }

        sessionsContainer.appendChild(li);
    })
}

async function updateFiles(connection) {

    if (!document.querySelector(".file-container")) {
        return;
    }

    var hrefSplitted = window.location.href.split("/");

    if (!hrefSplitted[hrefSplitted.length - 1]) {
        return;
    }

    $('.uploaded-file').remove();
    $('.pending-file').remove();

    var files = await connection.invoke("GetFiles", hrefSplitted[hrefSplitted.length - 1]);

    files.forEach(
        file => {
            const fileElement = $(`
                <div class="file p-2 uploaded-file btn-dark btn d-flex flex-column align-items-center justify-content-center" fileId="${file.id}" style="border-radius: 15px; width: 150px; height: 200px;">
                    <div class="d-flex w-100 align-items-center justify-content-center" style="flex: 0 0 33%">
                        <p><span class="sended-size">${file.sizeMb}</span> <span class="subtitle" style="font-size: 0.7em; letter-spacing: 1px;">MB</span></p>
                    </div>
                    <div class="d-flex w-100 align-items-center justify-content-center" style="flex: 0 0 33%">
                        <i class="fa-regular fa-file" style="font-size: 6em"></i>
                    </div>
                    <div class="d-flex w-100 align-items-center text-truncate justify-content-center" style="flex: 0 0 33%">
                        ${file.fileName}
                    </div>
                </div>
            `);
            $('.file-container').append(fileElement);

            fileElement.on("click", function (event) {
                new window.bootstrap.Button(fileElement).toggle();
                if (event.target.closest(".file").getAttribute("fileid")) {
                    toggleArrayElement(toggledFiles, event.target.closest(".file").getAttribute("fileid"))
                }

                if (filesCounter) {
                    filesCounter.innerText = "Выбрано " + toggledFiles.length + " Файлов";
                }

                if (!filesoffcanvasObj && filesoffcanvas) {
                    filesoffcanvasObj = new window.bootstrap.Collapse(filesoffcanvas)
                }

                if (filesoffcanvas) {
                    if (toggledFiles.length > 0) {
                        filesoffcanvasObj.show();
                    } else {
                        filesoffcanvasObj.hide();
                    }
                }
            })
        }
    )

    var uploads = (await connection.invoke("GetUploads", hrefSplitted[hrefSplitted.length - 1]));

    uploads.forEach(
        file => {
            const fileElement = $(`
                <div class="file p-2 pending-file btn-dark btn d-flex flex-column align-items-center justify-content-center" fileId="${file.id}" style = "border-radius: 15px; width: 150px; height: 200px;" >
                    <div class="d-flex flex-column w-100 align-items-center justify-content-center" style="flex: 0 0 33%">
                        <div class="progress" style="flex: 0 0 15%; width: 75%">
                            <div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" style="width: ${(file.sendedSize / file.size) * 100}%"></div>
                        </div>
                        <p><span class="sended-size">${file.sendedSizeMb}</span> / ${file.sizeMb} <span class="subtitle" style="font-size: 0.7em; flex: 0 0 85%; letter-spacing: 1px;">MB</span></p>
                    </div>
                    <div class="spinner-border" role="status" aria-hidden="true"></div>
                    <div class="d-flex w-100 align-items-center text-truncate justify-content-center" style="flex: 0 0 33%">
                        ${file.fileName}
                    </div>
                </div>`);
            $('.file-container').append(fileElement);
        }
    )

    countSesisonSize();
}

function collectCode(boxes) {
    // Получаем значения всех input-элементов
    let code = '';

    // Проходимся по каждому input и собираем значения
    boxes.forEach(input => {
        code += input.value;
    });

    // Выводим результат
    return code;
}