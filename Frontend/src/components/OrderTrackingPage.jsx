import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { easyBuyApi } from '../api/easyBuyApi'
import { heroMedia } from '../data/productMedia'

const OrderTrackingPage = ({ orders, session, formatCurrency, formatDate }) => {
  const navigate = useNavigate()
  const { orderId } = useParams()
  const [orderDetail, setOrderDetail] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const activeOrderId = useMemo(() => {
    if (orderId) {
      return Number(orderId)
    }

    return orders.items[0]?.id ?? null
  }, [orderId, orders.items])

  useEffect(() => {
    if (!activeOrderId) {
      setLoading(false)
      setOrderDetail(null)
      return
    }

    let cancelled = false

    const loadOrder = async () => {
      setLoading(true)
      setError(null)

      try {
        const detail = await easyBuyApi.getOrder(activeOrderId)
        if (!cancelled) {
          setOrderDetail(detail)
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError.message)
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    loadOrder()

    return () => {
      cancelled = true
    }
  }, [activeOrderId])

  useEffect(() => {
    if (!orderId && activeOrderId) {
      navigate(`/tracking/${activeOrderId}`, { replace: true })
    }
  }, [activeOrderId, navigate, orderId])

  return (
    <main className="tracking-page-shell">
      <section className="tracking-hero">
        <div className="tracking-hero-copy">
          <span className="section-kicker">Order tracking</span>
          <h1>Track every shipment milestone in one place.</h1>
          <p>
            Review courier updates, estimated delivery, payment state, and your full order summary
            without going back to checkout.
          </p>
          {!session ? <Link to="/login">Login to keep your order history synced</Link> : null}
        </div>
      </section>

      <div className="tracking-layout">
        <aside className="tracking-sidebar">
          <article className="tracking-sidebar-card">
            <span className="section-kicker">Your orders</span>
            <h2>{orders.items.length} order{orders.items.length === 1 ? '' : 's'}</h2>

            {orders.items.length === 0 ? (
              <div className="tracking-empty">
                <p>No orders available yet.</p>
                <Link to="/">Start shopping</Link>
              </div>
            ) : (
              <div className="tracking-order-list">
                {orders.items.map((order) => (
                  <button
                    key={order.id}
                    type="button"
                    className={`tracking-order-button ${order.id === activeOrderId ? 'active' : ''}`}
                    onClick={() => navigate(`/tracking/${order.id}`)}
                  >
                    <div>
                      <strong>{order.orderNumber}</strong>
                      <small>{formatDate(order.orderDate)}</small>
                    </div>
                    <div>
                      <span>{order.trackingStage}</span>
                      <small>{formatCurrency(order.total)}</small>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </article>
        </aside>

        <section className="tracking-content">
          {loading ? (
            <article className="tracking-main-card">
              <h2>Loading tracking details...</h2>
            </article>
          ) : error ? (
            <article className="tracking-main-card">
              <h2>Unable to load order tracking</h2>
              <p>{error}</p>
            </article>
          ) : !orderDetail ? (
            <article className="tracking-main-card">
              <h2>No order selected</h2>
              <p>Choose an order from the list to view tracking details.</p>
            </article>
          ) : (
            <>
              <article className="tracking-main-card">
                <div className="tracking-summary-head">
                  <div>
                    <span className="section-kicker">Shipment overview</span>
                    <h2>{orderDetail.orderNumber}</h2>
                    <p>{orderDetail.trackingStage}</p>
                  </div>
                  <div className="tracking-badges">
                    <span>{orderDetail.status}</span>
                    <span>{orderDetail.paymentStatus}</span>
                  </div>
                </div>

                <div className="tracking-info-grid">
                  <article>
                    <small>Tracking number</small>
                    <strong>{orderDetail.trackingNumber ?? 'Pending assignment'}</strong>
                  </article>
                  <article>
                    <small>Courier</small>
                    <strong>{orderDetail.courierName ?? 'EasyBuy fulfilment'}</strong>
                  </article>
                  <article>
                    <small>Estimated delivery</small>
                    <strong>
                      {orderDetail.estimatedDeliveryDate
                        ? formatDate(orderDetail.estimatedDeliveryDate)
                        : 'To be confirmed'}
                    </strong>
                  </article>
                  <article>
                    <small>Shipping address</small>
                    <strong>{orderDetail.shippingAddress}</strong>
                  </article>
                </div>
              </article>

              <article className="tracking-main-card">
                <div className="panel-head">
                  <div>
                    <span className="section-kicker">Tracking timeline</span>
                    <h2>Shipment events</h2>
                  </div>
                </div>

                <div className="tracking-timeline">
                  {orderDetail.trackingEvents.map((event, index) => (
                    <div key={`${event.code}-${event.occurredAt}`} className="tracking-event-row">
                      <div className={`tracking-event-marker ${event.completed ? 'done' : 'pending'}`}>
                        {index + 1}
                      </div>
                      <div className="tracking-event-copy">
                        <div className="tracking-event-head">
                          <strong>{event.title}</strong>
                          <small>{formatDate(event.occurredAt)}</small>
                        </div>
                        <p>{event.description}</p>
                        <span>{event.location}</span>
                      </div>
                    </div>
                  ))}
                </div>
              </article>

              <article className="tracking-main-card">
                <div className="panel-head">
                  <div>
                    <span className="section-kicker">Order contents</span>
                    <h2>Items in this shipment</h2>
                  </div>
                </div>

                <div className="tracking-item-list">
                  {orderDetail.items.map((item) => (
                    <div key={`${orderDetail.id}-${item.productId}`} className="tracking-item-row">
                      <img
                        src={item.imageUrl || heroMedia.deals}
                        alt={item.productName}
                        loading="lazy"
                      />
                      <div>
                        <strong>{item.productName}</strong>
                        <small>
                          Qty {item.quantity} · {formatCurrency(item.unitPrice)} each
                        </small>
                      </div>
                      <strong>{formatCurrency(item.lineTotal)}</strong>
                    </div>
                  ))}
                </div>
              </article>
            </>
          )}
        </section>
      </div>
    </main>
  )
}

export default OrderTrackingPage
