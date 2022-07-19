import React, { useState } from "react";
import OutputModal from "./components/OutputModal";
import "./App.css";

function App() {
  const [city, setCity] = useState("");
  const [country, setCountry] = useState("");
  const [output, setOutput] = useState("");
  const [valid, setValid] = useState(true);
  const [loading, setLoading] = useState(false);
  const [modalShow, setModalShow] = useState(false);

  const handleCityInputChange = (event) => {
    setCity(event.target.value);
  };

  const handleCountryInputChange = (event) => {
    setCountry(event.target.value);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (city && country) {
      setValid(true);
      setLoading(true);
      try {
        let res = await fetch(
          "http://localhost:5001/weather/description?" +
            new URLSearchParams({
              city: city,
              country: country,
            }),
          {
            method: "GET",
            headers: {
              ApiKey: "c4236b3b569840f6869bad373fbd7e7e",
            },
          }
        );
        if (res.status === 200) {
          let resJson = await res.json();
          setOutput(resJson);
        }
        else if (res.status === 204) {
          setOutput("No weather report available");
        }
         else if (res.status === 404 || res.status === 429 || res.status === 500) {
          let resText = await res.text();
          setOutput(resText);
        }
      } catch (err) {
        console.log(err);
        setOutput("Error contacting weather service.");
      }
      setLoading(false);
      setModalShow(true);
    } else {
      setValid(false);
    }
  };

  return (
    <div>
      <div class="form-container">
        <form class="form-horizontal" onSubmit={handleSubmit}>
          <h3 class="title">Weather Describer</h3>
          <span class="description">Get a description of the weather</span>
          <div class="form-group">
            <input
              onChange={handleCityInputChange}
              value={city}
              maxLength="60"
              class="form-control"
              placeholder="Enter city"
            />
            {!valid && !city ? <span class="error">*Required</span> : null}
          </div>
          <div class="form-group">
            <input
              onChange={handleCountryInputChange}
              value={country}
              maxLength="10"
              class="form-control"
              placeholder="Enter country code"
            />
            {!valid && !country ? <span class="error">*Required</span> : null}
          </div>
          <button class="btn" disabled={loading}>
            Get Weather
          </button>
        </form>
      </div>
      <OutputModal
        show={modalShow}
        onHide={() => setModalShow(false)}
        output={output}
      />
    </div>
  );
}

export default App;
