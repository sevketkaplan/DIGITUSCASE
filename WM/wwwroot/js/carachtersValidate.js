


//onpaste = "testCarachter('', '0123456789.', '')";
//onkeypress = "testCarachter('','0123456789.','')"
function testCarachter(invalidCarachters, validCarachters, regexText) {
    let e = this.event;
    // invalid ile carachter test
    if (invalidCarachters != null && invalidCarachters != undefined && invalidCarachters != "") {
        if (e.type === 'paste') {
            let value = e.clipboardData.getData('text');
            for (let i = 0; i < invalidCarachters.length; i++) {
                if (value.includes(invalidCarachters.charAt(i))) {
                    e.preventDefault();
                    break;
                }
            }
        }
        else {
            if (invalidCarachters.includes(e.key)) {
                e.preventDefault()
            }
        }
    }
    // valid carachter ile test işlemi
    else if (validCarachters != null && validCarachters != undefined && validCarachters != "") {
        if (e.type === 'paste') {
            let value = e.clipboardData.getData('text');
            for (let i = 0; i < validCarachters.lenght; i++) {
                if (!value.includes(validCarachters.charAt(i))) {
                    e.preventDefault();
                    break;
                }
            }
        }
        else {
            if (!validCarachters.includes(e.key)) {
                e.preventDefault();
            }
        }
    }
    else {// regex ile carachter test
        let reg = new RegExp(regexText);
        if (e.type === 'paste') {
            let value = e.clipboardData.getData('text');
            let state = reg.test(value);
            console.log(state);
            if (!state) {
                e.preventDefault();
            }

        }
        else {
            let state = reg.test(e.key);
            if (!state) {
                e.preventDefault();
            }
        }
    }

};