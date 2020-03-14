importScripts('deps/babel/babel.min.js');

//this is needed to activate the worker immediately without reload
//@see https://developers.google.com/web/fundamentals/primers/service-workers/lifecycle#clientsclaim
self.addEventListener('activate', event => event.waitUntil(clients.claim()));
const globalMap = {
    'react': 'React',
    'react-dom': 'ReactDOM',
    'signalr': 'signalR'
};
const getGlobalByUrl = (url) => Object.keys(globalMap).reduce((res, key) => {
    if (res) return res;
    if (matchUrl(url, key)) return globalMap[key];
    return res;
}, null);
const matchUrl = (url, key) => url.includes(`/${key}/`);
self.addEventListener('fetch', (event) => {
    let { request: { url } } = event;
    console.log('Req', url);
    const fileName = url.split('/').pop();
    const ext = fileName.includes('.') ? url.split('.').pop() : '';
    // if (!ext && !url.endsWith('/')) {
    //     url = url + '.jsx';
    // }
    if (globalMap && Object.keys(globalMap).some(key => matchUrl(url, key))) {
        event.respondWith(
            fetch(url)
                .then(response => response.text())
                .then(body => new Response(`
                    const head = document.getElementsByTagName('head')[0];
                    const script = document.createElement('script');
                    script.setAttribute('type', 'text/javascript');
                    script.appendChild(document.createTextNode(
                        ${JSON.stringify(body)}
                    ));
                    head.appendChild(script);
                    export default window.${getGlobalByUrl(url)};
                    `, {
                    headers: new Headers({
                        'Content-Type': 'application/javascript'
                    })
                })
                )
        )
        // } else if (url.endsWith('.js')) { // rewrite for import('./Panel') with no extension
        //     event.respondWith(
        //         fetch(url)
        //             .then(response => response.text())
        //             .then(body => new Response(
        //                 body,
        //                 {
        //                     headers: new Headers({
        //                         'Content-Type': 'application/javascript'
        //                     })
        //                 })
        //             )
        //     )
    } else if (url.endsWith('.jsx')) {
        event.respondWith(
            fetch(url)
                .then(response => response.text())
                .then(body => new Response(
                    //TODO Cache
                    Babel.transform(body, {
                        presets: [
                            'react',
                        ],
                        plugins: [
                            'syntax-dynamic-import'
                        ],
                        sourceMaps: true
                    }).code,
                    {
                        headers: new Headers({
                            'Content-Type': 'application/javascript'
                        })
                    })
                )
        )
    }
});