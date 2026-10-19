// Po upływie limitu bezczynności przechodzi na stronę logowania - serwer i tak odrzuci już tę sesję.
(() => {
    let timer;

    function arm() {
        clearTimeout(timer);
        const el = document.getElementById('session-timeout');
        if (!el) return;
        const ms = parseFloat(el.dataset.minutes) * 60000 + 2000;
        timer = setTimeout(() => { location.href = 'Account/Login?reason=timeout'; }, ms);
    }

    arm();
    Blazor.addEventListener('enhancedload', arm);
})();
