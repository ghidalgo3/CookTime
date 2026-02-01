import $ from "jquery";

let url = window.location.href
let anchor = url.substring(url.indexOf("#"))
$("div.card").hide()
if (anchor != window.location.href) {
    $(`${anchor}`).show()
} else {
    $("div.card").first().show()
}

($ as any).validator.addMethod(
    'password',
    function (value, element, params) {
        // console.log(element)
        if (element.id === "passwordInput") {
            for (let index = 0; index < value.length - 1; index++) {
                if (value.charAt(index) !== value.charAt(index + 1)) {
                    return true;
                }
            }
            return false;
        }
        return true;
    },
    "Password must have at least 2 unique characters.");