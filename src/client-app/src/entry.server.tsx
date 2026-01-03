import type { EntryContext } from "react-router";
import { ServerRouter } from "react-router";
import { renderToString } from "react-dom/server";

export default function handleRequest(
  request: Request,
  responseStatusCode: number,
  responseHeaders: Headers,
  routerContext: EntryContext
) {
  // In SPA mode, just return a basic HTML shell
  // The client will handle all rendering
  const body = renderToString(
    <ServerRouter context={routerContext} url={request.url} />
  );

  responseHeaders.set("Content-Type", "text/html");

  return new Response("<!DOCTYPE html>" + body, {
    headers: responseHeaders,
    status: responseStatusCode,
  });
}
