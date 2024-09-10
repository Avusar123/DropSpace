var toggledButtons = [];

const filesoffcanvas = document.getElementById("navbarToggleExternalContent");

const filesCounter = document.getElementById("files-counter")

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

connection.on("newUser", function (session) {
    var toast = new window.bootstrap.Toast($("#primary-toast"));
    $("#primary-toast-body").text("К сессии присоединился новый пользователь!")
    $("#memberCount").text(session.membersCount);
    toast.show();
})
connection.on("UserLeave", function (session) {
    var toast = new window.bootstrap.Toast($("#primary-toast"));
    $("#primary-toast-body").text("Пользователь покинул сессию!")
    $("#memberCount").text(session.membersCount);
    toast.show();
})

connection.on("ErrorRecieved", function (err) {
    showError(err)
})

connection.start().catch(function (err) {
    return console.error(err.toString());
}).then(() => {
    updateSessions(connection);
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
                const code = urlParams.get('code');

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