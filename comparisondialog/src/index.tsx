import ReactDOM from 'react-dom';
import { App } from './App';
import { initializeIcons } from '@fluentui/react/lib/Icons';

initializeIcons();

const queryString = window.location.search.substring(1);
let params: any = {};
const queryStringParts = queryString.split("&");
for (let i = 0; i < queryStringParts.length; i++) {
  const pieces = queryStringParts[i].split("=");
  params[pieces[0].toLowerCase()] = pieces.length === 1 ? null : decodeURIComponent(pieces[1]);
}

var requestObject = {
  "InData": params.data
};

fetch("/api/data/v9.1/ab_Compare", {
  method: "POST",
  headers: {
    'OData-MaxVersion': '4.0',
    'OData-Version': '4.0',
    'Accept': 'application/json',
    'Content-Type': 'application/json; charset=utf-8'
  },
  body: JSON.stringify(requestObject)
}).then(response => response.json())
.then(data => {
  const records = JSON.parse(data.OutData);
  ReactDOM.render(
    <App 
      Data={records}
    />,
  document.getElementById('root')
);
})
.catch(e => {
  document.getElementById('root')!.innerHTML = `Something went wrong during initialization of the form - ${e}`;
});

