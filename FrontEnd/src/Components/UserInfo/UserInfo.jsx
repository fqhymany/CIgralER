import {useQuery, useMutation, useQueryClient} from '@tanstack/react-query';
import userPic from '../../assets/userPic.jpg';
import api from '../../api/axios';

const userData = {
  userName: 'تست',
  companyName: 'تست',
  userImg: userPic,
};

// Fetch user profile from the database
const fetchUserProfile = async () => {
  const response = await api.get('/api/AuthEndpoints/profile');
  if (response.status !== 200) {
    throw new Error('Error fetching case types');
  }
  return response.data;
};

function UserInfo(props) {
  // Fetch data using React Query's useQuery hook
  const {data, error, isLoading} = useQuery({
    queryKey: ['userProfile'],
    queryFn: fetchUserProfile,
  });
  return (
    <div className={`d-flex gap-2 col-5 ${props.display}`}>
      <img src={userData.userImg} alt="" className="user-img rounded-circle" />
      <span className="fs-7">
        <span className="fw-bold d-none d-md-inline"></span>
        {data?.fullName || ""}
      </span>
      <span className="fs-7 border-white border-md-dark border-end px-2">
        <span className="fw-bold d-none d-md-inline">دفتر مجازی : </span>
        {data?.regionName || "نامشخص"}
      </span>
    </div>
  );
}

export default UserInfo;
