import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom'
import { sriLankaDistricts } from '../data/sriLankaDistricts'

const MarketplaceHeader = ({
  session,
  cartCount,
  search,
  setSearch,
  deliveryLocation,
  setDeliveryLocation,
  onLogout,
}) => {
  const location = useLocation()
  const navigate = useNavigate()
  const isAuthRoute = location.pathname === '/login' || location.pathname === '/register'

  const handleSearchSubmit = (event) => {
    event.preventDefault()
    navigate('/')
  }

  return (
    <header className="market-header">
      <div className="market-header-top">
        <div className="market-brand-cluster">
          <Link to="/" className="market-logo">
            <span>EB</span>
            <div>
              <strong>EasyBuy</strong>
              <small>Online marketplace</small>
            </div>
          </Link>

          <label className="market-delivery">
            <small>Deliver to</small>
            <select
              value={deliveryLocation}
              onChange={(event) => setDeliveryLocation(event.target.value)}
              aria-label="Delivery district"
            >
              {sriLankaDistricts.map((district) => (
                <option key={district} value={district}>
                  {district}
                </option>
              ))}
            </select>
          </label>
        </div>

        {!isAuthRoute ? (
          <form className="market-search" onSubmit={handleSearchSubmit}>
            <select defaultValue="all" aria-label="department">
              <option value="all">All</option>
              <option value="audio">Audio</option>
              <option value="workstations">Workstations</option>
              <option value="home-office">Home Office</option>
              <option value="wearables">Wearables</option>
            </select>
            <input
              type="search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search EasyBuy for keyboards, webcams, speakers, and more"
            />
            <button type="submit">Search</button>
          </form>
        ) : (
          <div className="market-auth-copy">
            <strong>Secure account access</strong>
            <small>Sign in or create an account to continue shopping.</small>
          </div>
        )}

        <div className="market-account-links">
          <NavLink to={session ? '/' : '/login'} className="header-link-card">
            <small>Hello, {session ? session.name.split(' ')[0] : 'sign in'}</small>
            <strong>Account</strong>
          </NavLink>

          <NavLink to="/" className="header-link-card">
            <small>Returns</small>
            <strong>& Orders</strong>
          </NavLink>

          <NavLink to="/checkout" className="header-cart-link">
            <small>Cart</small>
            <strong>{cartCount}</strong>
          </NavLink>

          {session ? (
            <button type="button" className="header-signout" onClick={onLogout}>
              Sign out
            </button>
          ) : null}
        </div>
      </div>

      <div className="market-header-subnav">
        <nav>
          <NavLink to="/">Today's Deals</NavLink>
          <NavLink to="/">Categories</NavLink>
          <NavLink to="/">Best Sellers</NavLink>
          <NavLink to="/">Orders</NavLink>
          <NavLink to="/">Admin</NavLink>
          <NavLink to="/tracking">Tracking</NavLink>
          <NavLink to="/checkout">Checkout</NavLink>
          <NavLink to="/login">Login</NavLink>
          <NavLink to="/register">Register</NavLink>
        </nav>
        <span>Marketplace UI inspired by major retail layouts, adapted for EasyBuy.</span>
      </div>
    </header>
  )
}

export default MarketplaceHeader
