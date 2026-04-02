import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'

const AuthPage = ({ mode, heroMedia, onSubmitAuth }) => {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    name: mode === 'register' ? 'Ayesha Fernando' : 'Ayesha Fernando',
    email: 'ayesha@easybuy.demo',
    password: 'easybuy123',
  })

  const isRegister = mode === 'register'

  const submit = (event) => {
    event.preventDefault()
    onSubmitAuth(form)
    navigate('/')
  }

  return (
    <main className="auth-page-shell">
      <section className="auth-visual-panel">
        <img src={heroMedia.auth} alt="EasyBuy account visual" />
        <div className="auth-overlay">
          <span className="section-kicker">EasyBuy Account</span>
          <h1>{isRegister ? 'Create your customer account' : 'Login to continue shopping'}</h1>
          <p>
            Dedicated login and register pages now exist as part of the storefront, instead of
            being missing from the UI.
          </p>
        </div>
      </section>

      <section className="auth-form-panel">
        <div className="auth-form-card">
          <span className="section-kicker">{isRegister ? 'Register' : 'Login'}</span>
          <h2>{isRegister ? 'Start your EasyBuy profile' : 'Welcome back'}</h2>
          <p>Use the demo credentials or adjust them. Authentication is stored locally in this UI.</p>

          <form className="auth-form" onSubmit={submit}>
            {isRegister ? (
              <label>
                <span>Full name</span>
                <input
                  type="text"
                  value={form.name}
                  onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
                />
              </label>
            ) : null}

            <label>
              <span>Email</span>
              <input
                type="email"
                value={form.email}
                onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
              />
            </label>

            <label>
              <span>Password</span>
              <input
                type="password"
                value={form.password}
                onChange={(event) =>
                  setForm((current) => ({ ...current, password: event.target.value }))
                }
              />
            </label>

            <button type="submit" className="primary-action wide-action">
              {isRegister ? 'Create account' : 'Login now'}
            </button>
          </form>

          <p className="auth-switch-copy">
            {isRegister ? 'Already have an account?' : 'Need a new account?'}{' '}
            <Link to={isRegister ? '/login' : '/register'}>
              {isRegister ? 'Login here' : 'Register here'}
            </Link>
          </p>
        </div>
      </section>
    </main>
  )
}

export default AuthPage
