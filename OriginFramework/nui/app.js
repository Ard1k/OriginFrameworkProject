


$(document).ready(function () {
  window.addEventListener('message', function (event) {
    var eventName = event.data.type;
    if (eventName === 'copyCoords') {
      var x = Number((event.data.x).toFixed(4));
      var y = Number((event.data.y).toFixed(4));
      var z = Number((event.data.z).toFixed(4));
      var str = x + " " + y + " " + z;
      //var str = "\"X\": " + x + ", \"Y\": " + y + ", \"Z\": " + z;
      console.log('Copied:' + str)
      copyTextToClipboard(str);
    }
    else if (eventName === 'copyCoordsAndHeading') {
      var x = Number((event.data.x).toFixed(4));
      var y = Number((event.data.y).toFixed(4));
      var z = Number((event.data.z).toFixed(4));
      var h = Number((event.data.h).toFixed(4));
      var str = x + " " + y + " " + z + " " + h;
      //var str = "\"X\": " + x + ", \"Y\": " + y + ", \"Z\": " + z + ", \"Heading\": " + h;
      console.log('Copied:' + str)
      copyTextToClipboard(str);
    }
    else if (eventName === 'copyDataMessage') {
      var str = event.data.message;
      console.log('Copied:' + str)
      copyTextToClipboard(str);
    }
    else if (eventName === 'inventoryNotification') {
      var amountClass = "positiveAmount";
      if (event.data.amount.charAt(0) === "-") {
        amountClass = "negativeAmount";
      }

      var content = $('<div class="inventoryNotificationItem" style="display: none"><div class="text">' + event.data.text + '</div><div class="' + amountClass + '">' + event.data.amount + '</div></div>')

      $('.inventoryNotifications').prepend(content)
      $(content).fadeIn(400)

      setTimeout(function () {
        $(content).fadeOut(1250)
      }, 3750)

      setTimeout(function () {
        $(content).css('display', 'none')
      }, 5000)
    }
    else if (eventName === 'showInventoryTooltip') {
      var header = event.data.header;
      var rows = event.data.rows;

      var tooltip = document.querySelector('.inventoryTooltip');
      tooltip.style.display = 'block';
      tooltip.style.top = event.data.y * 100 + 'vh';
      tooltip.style.left = event.data.x * 100 + 'vw';

      var headerDiv = tooltip.querySelector('.tooltipHeader');
      headerDiv.innerHTML = header;

      if (event.data.bgcolor === 'red')
      {
        headerDiv.style.backgroundColor = '#ff0000aa';
      }
      else if (event.data.bgcolor === 'blue') {
        headerDiv.style.backgroundColor = '#0000ffaa';
      }
      else if (event.data.bgcolor === 'black') {
        headerDiv.style.backgroundColor = '#000000aa';
      }

      var rowsDiv = tooltip.querySelector('.tooltipRows');
      rowsDiv.innerHTML = '';

      for (var i = 0; i < rows.length; i++) {
        var rowDiv = document.createElement('div');

        if (rows[i].s1 != null)
          rowDiv.innerHTML = '<div class="tooltipRow"><span>' + rows[i].s1 + ':</span> ' + rows[i].s2 + "</div>";
        else
          rowDiv.innerHTML = '<div class="tooltipRow">' + rows[i].s2 + "</div>";

        rowsDiv.appendChild(rowDiv);
      }
    }
    else if (eventName === 'hideInventoryTooltip') {
      var tooltip = document.querySelector('.inventoryTooltip');
      tooltip.style.display = 'none';
    }
  })
})

function copyTextToClipboard(text) {
  var copyFrom = $('<textarea/>');
  copyFrom.text(text);
  $('body').append(copyFrom);
  copyFrom.select();
  document.execCommand('copy');
  copyFrom.remove();
}