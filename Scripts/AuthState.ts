export function isSignedIn() : boolean {
    let authState = document.getElementById("authState")
    if (authState == null) {
        return false
    }
    var isSignedIn = authState.getAttribute("data-signed-in")
    return isSignedIn === "True"
}

export function getUserId() : string | null {
    let authState = document.getElementById("authState")
    if (authState == null) {
        return null
    }
    return authState.getAttribute("data-user-id")
}