import './App.css'

function App() {
  return (
    <>
      <header className="app-header">
        <h1>Kursy walut</h1>
        <p className="app-subtitle">Przeliczanie kursów walut i ich historia na podstawie danych dostawców kursów.</p>
      </header>

      <main className="app-main">{/* The conversion form and the results are added in the next steps. */}</main>

      <footer className="app-footer">
        Dane: <a href="https://api.nbp.pl/">API Narodowego Banku Polskiego</a>
      </footer>
    </>
  )
}

export default App
