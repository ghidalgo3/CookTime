import {
    Links,
    Meta,
    Outlet,
    Scripts,
    ScrollRestoration,
} from "react-router";
import { HelmetProvider } from 'react-helmet-async';
import { AuthenticationContext } from './components/Authentication/AuthenticationContext';
import { FavoritesProvider } from './components/Favorites/FavoritesContext';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import '@smastrom/react-rating/style.css';
import siteStyles from './assets/css/site.css?url';

// Initialize Application Insights
const appInsights = new ApplicationInsights({
    config: {
        connectionString: 'InstrumentationKey=b37afa75-076b-4438-a84d-79b9f4617d30;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus2.livediagnostics.monitor.azure.com/',
        enableAutoRouteTracking: true,
        enableCorsCorrelation: true,
        enableRequestHeaderTracking: true,
        enableResponseHeaderTracking: true,
        correlationHeaderExcludedDomains: ['*.queue.core.windows.net']
    }
});
appInsights.loadAppInsights();
appInsights.trackPageView();

export function Layout({ children }: { children: React.ReactNode }) {
    // Detect color scheme for Bootstrap 5.3+ dark mode
    const colorSchemeScript = `
        (function() {
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            document.documentElement.setAttribute('data-bs-theme', prefersDark ? 'dark' : 'light');
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
                document.documentElement.setAttribute('data-bs-theme', e.matches ? 'dark' : 'light');
            });
        })();
    `;

    return (
        <html lang="en">
            <head>
                <script dangerouslySetInnerHTML={{ __html: colorSchemeScript }} />
                <script
                    async
                    src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-6004231239349931"
                    crossOrigin="anonymous"></script>
                <meta charSet="utf-8" />
                <link rel="icon" href="/favicon.ico" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <meta name="theme-color" content="#000000" />
                <meta name="description" content="CookTime - Discover and share delicious recipes" />
                <link rel="apple-touch-icon" href="/logo192.png" />
                <link rel="manifest" href="/manifest.json" />
                <link
                    rel="stylesheet"
                    href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.5/dist/css/bootstrap.min.css"
                    integrity="sha384-SgOJa3DmI69IUzQ2PVdRZhwQ+dy64/BUtbMJw1MZ8t5HZApcHrRKUc4W0kG879m7"
                    crossOrigin="anonymous"
                />
                <link
                    rel="stylesheet"
                    href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.13.1/font/bootstrap-icons.min.css"
                />
                <link rel="stylesheet" href={siteStyles} />
                <Meta />
                <Links />
            </head>
            <body>
                <noscript>You need to enable JavaScript to run this app.</noscript>
                {children}
                <ScrollRestoration />
                <Scripts />
            </body>
        </html>
    );
}

export function HydrateFallback() {
    return (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '100vh',
            fontFamily: 'system-ui'
        }}>
            Loading...
        </div>
    );
}

export default function Root() {
    return (
        <HelmetProvider>
            <AuthenticationContext>
                <FavoritesProvider>
                    <Outlet />
                </FavoritesProvider>
            </AuthenticationContext>
        </HelmetProvider>
    );
}
