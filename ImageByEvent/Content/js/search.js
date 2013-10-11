$("#searchGetty").click(function () {
    $.post('/home/Search',
        {
            term: $("#searchGettyInputBox").val()
        },
        function (data) {
            if (data == "") {
                alert('Error! Something went wrong');
            } else {                
                poll(data);
            }            
        });

    return false;
});

function poll(id) {
    $.post('/home/Ping', { sqsId: id },
        function (data) {
            console.log("PING " + data);
            if (data == "") {
                setTimeout("poll('" + id + "')", 5000);
                console.log("poll");
            }
            else if(data != "") {
                console.log("DATA" + data);
                console.log(data)
            }           
        });
}

//$(".smallBox").hover(function () {
//    console.log(this);
//});

$(document).ready(function () {
    $('.smallBox').tipr();
});