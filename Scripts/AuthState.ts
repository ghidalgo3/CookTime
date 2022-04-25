export function isSignedIn() {
    let authState = document.getElementById("authState")
    if (authState == null) {
        return false
    }
    var isSignedIn = authState.getAttribute("data-signed-in")
    return isSignedIn === "True"
}