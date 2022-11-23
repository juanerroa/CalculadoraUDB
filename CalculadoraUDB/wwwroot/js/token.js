var endDate, countDown, urlExtender;

const cookieAccesoName = '__RequestVerificationTokenUDB';

const showExtenderModal = () => {
    countDown = setInterval(() => {
        var start = new Date();
        if (start >= endDate) {
            logOut_Auth();
        }
        else {
            var diff = Math.round((endDate.getTime() - start.getTime()) / 1000);
            var hours = Math.floor(diff / 3600);
            diff -= hours * 3600
            var minutes = Math.floor(diff / 60);
            diff -= minutes * 60;
            var seconds = (diff % 60).length == 1 ? `0${(diff % 60).length}` : (diff % 60);
            document.getElementById('session_countdown').innerHTML = `${minutes} Minutos con ${seconds} Segundos`;
        }
    }, 1000);

    setTimeout(() => {
        if (!document.getElementById('modalExtenderSesion').classList.contains('show'))
            modalExtenderSesion.show();
    }, 1000)

}

const modalExtenderSesion = new bootstrap.Modal(document.getElementById('modalExtenderSesion'));

const eliminarCookie = () => document.cookie = `${cookieAccesoName}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;`;

const getTokenAuth = () => {
    if (document.cookie.length > 0) {
        let c_start = document.cookie.indexOf(cookieAccesoName + "=");
        if (c_start != -1) {
            c_start = c_start + cookieAccesoName.length + 1;
            let c_end = document.cookie.indexOf(";", c_start);
            if (c_end == -1) {
                c_end = document.cookie.length;
            }
            return unescape(document.cookie.substring(c_start, c_end));
        }
    }
    return "";
}

const logOut_Auth = () => {
    modalExtenderSesion.hide();
    eliminarCookie();
    window.location.href = `/login/logout`;
}

const stopCountDown = () => clearInterval(countDown);

export const initTiempoExpiracion = (instance, _urlExtender) => {

    let url = encodeURIComponent(window.location.href);

    urlExtender = _urlExtender;

    var source = new EventSource(`http://localhost:5130/api/Login/CheckAuth/${getTokenAuth()}/${url}/${instance}`);

    source.onmessage = function (event) {
        var start = new Date();
        endDate = new Date(event.data);

        var diff = Math.round((endDate.getTime() - start.getTime()) / 1000);
        var hours = Math.floor(diff / 3600);
        diff -= hours * 3600
        var minutes = Math.floor(diff / 60);
        diff -= minutes * 60;

        if (minutes <= 5)
            showExtenderModal();
    }

    source.onerror = function (event) {
        source.close();
    }
}


//Eventos

document.querySelector('a[href$="/Login/Logout"]').addEventListener('click', () => eliminarCookie());

document.querySelectorAll('#modalExtenderSesion [data-bs-dismiss="modal"]').forEach(x => x.addEventListener('click', () => logOut_Auth()));

document.querySelector('#BTNExtenderAuth').addEventListener('click', async () => {
    $('#loader').show();
    const settings = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        }
    };
    const fetchResponse = await fetch(urlExtender, settings);
    const response = await fetchResponse.json();
    modalExtenderSesion.hide();
    stopCountDown();
    $('#loader').hide();
});


