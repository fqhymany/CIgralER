import {useState, useEffect, useMemo, useCallback} from 'react';
import api from '../api/axios';
import {useQuery, useMutation, useQueryClient} from '@tanstack/react-query';
import {Spinner, Alert, Button} from 'react-bootstrap';
import {Link, useLocation} from 'react-router-dom';
import {PiUserPlusLight} from 'react-icons/pi';
import useDevice from '../customHooks/useDevice';
import ClientsMobile from '../components/ClientUsers/ClientsMobile';
import ClientsDesktop from '../components/ClientUsers/ClientsDesktop';
import Search from '../components/Search/Search';
import {ToastContainer} from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import 'sweetalert2/dist/sweetalert2.css';
import '../assets/styles/rtl-sweetalert.css'; // You'll need to create this CSS file

// Fetch existing Clients from the database
const fetchClients = async () => {
  const response = await api.get('/api/ClientEndpoints/Clients');
  if (response.status !== 200) {
    throw new Error('Error fetching Clients');
  }
  return response.data;
};

const Clients = () => {
  const {isDesktop, isMobile} = useDevice();
  const location = useLocation();
  const queryClient = useQueryClient();

  // If returning from UserManagement, refetch clients
  useEffect(() => {
    if (location.state?.refetchClients) {
      queryClient.invalidateQueries(['Clients']);
    }
  }, [location.state, queryClient]);

  const {data, error, isLoading} = useQuery({
    queryKey: ['Clients'],
    queryFn: fetchClients,
    staleTime: 0, // Always consider data stale to ensure fresh data
  });

  const [searchTerm, setSearchTerm] = useState('');

  const handleClientNameSearch = useCallback((term) => {
    setSearchTerm(term);
  }, []);

  const filteredClients = useMemo(() => {
    if (!data) return [];

    if (searchTerm.trim() === '') {
      return data;
    }

    return data.filter(
      (client) =>
        client.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        client.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (client.firstName.toLowerCase() + ' ' + client.lastName.toLowerCase()).includes(searchTerm.toLowerCase())
    );
  }, [data, searchTerm]);

  if (isLoading) return <Spinner animation="border" />;
  if (error instanceof Error) return <Alert variant="danger">Error: {error.message}</Alert>;

  return (
    <>
      <div className="position-relative px-2 py-3 px-md-1 d-flex flex-column justify-content-center align-items-center gap-md-2 gap-2 flex-wrap rounded">
        <div className="w-100 d-flex justify-content-between align-items-center px-3 mb-4 pe-0">
          <Search onSearch={handleClientNameSearch} placeholder="نام و نام‌خانوادگی" />
          <Link to="/UserManagement" className="btn btn-primary d-flex align-items-center gap-2 translate-x" style={{fontSize: '0.875rem', padding: '0.5rem 1rem'}}>
            <PiUserPlusLight className="fs-5" />
            <span> جدید </span>
          </Link>
        </div>
        {isDesktop ? <ClientsDesktop clientsData={filteredClients} /> : <ClientsMobile clientsData={filteredClients} />}
      </div>
      <ToastContainer position="bottom-left" rtl />
    </>
  );
};

export default Clients;
