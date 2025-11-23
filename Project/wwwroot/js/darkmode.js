/*
This script toggles dark mode for a Leaflet map and the webpage.
*/

document.addEventListener('DOMContentLoaded', () => {
    const map = window.map;  // Map must be created already
    const themeSwitch = document.getElementById('theme-switch');

    //if (!themeSwitch) return;  // No button, exit
    if (!map) {
        // No map found — fallback: just toggle body class and localStorage without map layers
        const isDark = localStorage.getItem('darkmode') === 'active';
        if (isDark) document.body.classList.add('darkmode');

        themeSwitch.addEventListener('click', () => {
            const isDarkNow = document.body.classList.contains('darkmode');
            if (isDarkNow) {
                document.body.classList.remove('darkmode');
                localStorage.setItem('darkmode', null);
            } else {
                document.body.classList.add('darkmode');
                localStorage.setItem('darkmode', 'active');
            }
        });
        return;
    }

    // Define tile layers
    window.lightLayer = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    });

    window.darkLayer = L.tileLayer(
        `https://tiles.stadiamaps.com/tiles/alidade_satellite/{z}/{x}/{y}{r}.jpg?api_key=${stadiaApiKey}`,
        {
            minZoom: 0,
            maxZoom: 20,
            attribution: '&copy; Stadia Maps & OpenMapTiles & OpenStreetMap'
        }
    );

    // Apply correct base layer based on saved dark mode
    const isDark = localStorage.getItem('darkmode') === 'active';
    if (isDark) {
        document.body.classList.add('darkmode');
        window.darkLayer.addTo(map);
    } else {
        window.lightLayer.addTo(map);
    }

    // Toggle functions
    const enableMapDarkMode = () => {
        document.body.classList.add('darkmode');
        map.removeLayer(window.lightLayer);
        map.addLayer(window.darkLayer);
        localStorage.setItem('darkmode', 'active');
    };

    const disableMapDarkMode = () => {
        document.body.classList.remove('darkmode');
        map.removeLayer(window.darkLayer);
        map.addLayer(window.lightLayer);
        localStorage.setItem('darkmode', null);
    };

    // Handle theme switch click
    themeSwitch.addEventListener('click', () => {
        const darkmode = localStorage.getItem('darkmode');
        if (darkmode !== 'active') {
            enableMapDarkMode();
        } else {
            disableMapDarkMode();
        }
    });
});
