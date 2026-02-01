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

export function isAdmin() : boolean {
    let authState = document.getElementById("authState")
    if (authState == null) {
        return false
    }
    let roles = authState.getAttribute("data-roles") as string
    if (roles != null) {
        return roles.toUpperCase().includes("ADMINISTRATOR");
    } else {
        return false;
    }
}